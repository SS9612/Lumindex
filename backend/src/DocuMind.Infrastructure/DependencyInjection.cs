using DocuMind.Application.Authentication;
using DocuMind.Domain.Entities;
using DocuMind.Infrastructure.Identity;
using DocuMind.Infrastructure.Persistence;
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

        // TODO: Wire Azure Blob, Azure AI Search, Azure OpenAI, and Hangfire clients during week 2.
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
