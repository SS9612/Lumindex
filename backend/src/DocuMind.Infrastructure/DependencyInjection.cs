using Azure.Storage.Blobs;
using DocuMind.Application.Authentication;
using DocuMind.Application.Common.Persistence;
using DocuMind.Application.Common.Storage;
using DocuMind.Domain.Entities;
using DocuMind.Infrastructure.Identity;
using DocuMind.Infrastructure.Persistence;
using DocuMind.Infrastructure.Persistence.Repositories;
using DocuMind.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuMind.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDocuMindInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // SQLite for local dev — swap to Npgsql when wiring Postgres in production.
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=documind.db";

        services.AddDbContext<DocuMindDbContext>(options =>
        {
            options.UseSqlite(connectionString, sqlite =>
            {
                sqlite.MigrationsAssembly(typeof(DocuMindDbContext).Assembly.FullName);
            });
        });

        services.AddDocuMindIdentity();
        services.AddDocuMindRepositories();
        services.AddDocuMindBlobStorage(configuration);

        // TODO: Wire Azure AI Search, Azure OpenAI, and Hangfire clients during week 2.
        return services;
    }

    private static IServiceCollection AddDocuMindBlobStorage(this IServiceCollection services, IConfiguration configuration)
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

    private static IServiceCollection AddDocuMindRepositories(this IServiceCollection services)
    {
        // Expose the DbContext as the unit of work so handlers commit without depending on EF.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DocuMindDbContext>());

        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IChunkRepository, ChunkRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();

        return services;
    }

    private static IServiceCollection AddDocuMindIdentity(this IServiceCollection services)
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
            .AddEntityFrameworkStores<DocuMindDbContext>();

        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}
