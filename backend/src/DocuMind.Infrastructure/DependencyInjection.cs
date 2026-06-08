using DocuMind.Infrastructure.Persistence;
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

        // TODO: Wire Azure Blob, Azure AI Search, Azure OpenAI, and Hangfire clients during week 2.
        return services;
    }
}
