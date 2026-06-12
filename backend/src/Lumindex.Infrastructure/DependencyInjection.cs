using Azure.Storage.Blobs;
using Lumindex.Application.Authentication;
using Lumindex.Application.Common.Ingestion;
using Lumindex.Application.Common.Persistence;
using Lumindex.Application.Common.Storage;
using Lumindex.Application.Documents.Ingestion;
using Lumindex.Domain.Entities;
using Lumindex.Infrastructure.BackgroundJobs;
using Lumindex.Infrastructure.Identity;
using Lumindex.Infrastructure.Ingestion;
using Lumindex.Infrastructure.Persistence;
using Lumindex.Infrastructure.Persistence.Repositories;
using Lumindex.Infrastructure.Search;
using Lumindex.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lumindex.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLumindexInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // SQLite for local dev — swap to Npgsql when wiring Postgres in production.
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=lumindex.db";

        services.AddDbContext<LumindexDbContext>(options =>
        {
            options.UseSqlite(connectionString, sqlite =>
            {
                sqlite.MigrationsAssembly(typeof(LumindexDbContext).Assembly.FullName);
            });
        });

        services.AddLumindexIdentity();
        services.AddLumindexRepositories();
        services.AddLumindexBlobStorage(configuration);
        services.AddLumindexIngestion(configuration);

        return services;
    }

    private static IServiceCollection AddLumindexIngestion(this IServiceCollection services, IConfiguration configuration)
    {
        // The pipeline orchestrator + its provider-agnostic building blocks.
        services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();
        services.AddSingleton<IDocumentIngestionQueue, HangfireDocumentIngestionQueue>();
        services.AddSingleton<ITokenCounter, HeuristicTokenCounter>();
        services.AddSingleton<ITextExtractor, TextExtractor>();
        services.AddSingleton<ITextChunker, TokenAwareTextChunker>();

        services.AddLumindexEmbeddings(configuration);
        services.AddLumindexSearch(configuration);

        return services;
    }

    private static IServiceCollection AddLumindexEmbeddings(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AzureOpenAIOptions.SectionName);
        var options = new AzureOpenAIOptions
        {
            Endpoint = section["Endpoint"],
            ApiKey = section["ApiKey"],
            ChatDeployment = Fallback(section["ChatDeployment"], "gpt-4o-mini"),
            EmbeddingDeployment = Fallback(section["EmbeddingDeployment"], "text-embedding-3-small"),
            EmbeddingDimensions = ParseInt(section["EmbeddingDimensions"], 1536),
        };

        if (options.IsConfigured)
        {
            services.AddSingleton<IEmbeddingGenerator>(_ => new AzureOpenAIEmbeddingGenerator(options));
        }
        else
        {
            // Offline fallback so the ingestion pipeline runs without provisioning Azure OpenAI.
            services.AddSingleton<IEmbeddingGenerator>(_ => new DeterministicEmbeddingGenerator(options.EmbeddingDimensions));
        }

        return services;
    }

    private static IServiceCollection AddLumindexSearch(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AzureSearchOptions.SectionName);
        var options = new AzureSearchOptions
        {
            Endpoint = section["Endpoint"],
            ApiKey = section["ApiKey"],
            IndexName = Fallback(section["IndexName"], "lumindex-chunks"),
            VectorDimensions = ParseInt(section["VectorDimensions"], 1536),
            LocalPath = section["LocalPath"],
        };

        if (options.IsConfigured)
        {
            services.AddSingleton<ISearchIndexer>(_ => new AzureAiSearchIndexer(options));
        }
        else
        {
            // Local JSON index fallback mirrors the local blob-storage fallback for offline dev.
            var root = string.IsNullOrWhiteSpace(options.LocalPath)
                ? Path.Combine(AppContext.BaseDirectory, "App_Data", "search-index")
                : options.LocalPath;
            services.AddSingleton<ISearchIndexer>(_ => new LocalFileSearchIndex(root));
        }

        return services;
    }

    private static string Fallback(string? value, string @default) =>
        string.IsNullOrWhiteSpace(value) ? @default : value;

    private static int ParseInt(string? value, int @default) =>
        int.TryParse(value, out var parsed) && parsed > 0 ? parsed : @default;

    private static IServiceCollection AddLumindexBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AzureStorageOptions.SectionName);
        var connectionString = section["ConnectionString"];
        var containerName = section["ContainerName"];
        containerName = string.IsNullOrWhiteSpace(containerName) ? "documents" : containerName;

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton(_ => new BlobServiceClient(connectionString));
            services.AddSingleton<IBlobStorage>(sp =>
                new AzureBlobStorage(sp.GetRequiredService<BlobServiceClient>(), containerName));
        }
        else
        {
            // Local development fallback: persist uploads under the content root so the rest of the
            // ingestion pipeline can run without provisioning any Azure resources.
            var localPath = section["LocalPath"];
            var root = string.IsNullOrWhiteSpace(localPath)
                ? Path.Combine(AppContext.BaseDirectory, "App_Data", "blob-storage")
                : localPath;
            services.AddSingleton<IBlobStorage>(_ => new LocalFileBlobStorage(root));
        }

        return services;
    }

    private static IServiceCollection AddLumindexRepositories(this IServiceCollection services)
    {
        // Expose the DbContext as the unit of work so handlers commit without depending on EF.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LumindexDbContext>());

        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IChunkRepository, ChunkRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();

        return services;
    }

    private static IServiceCollection AddLumindexIdentity(this IServiceCollection services)
    {
        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<LumindexDbContext>();

        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}
