using System.Text;
using DocuMind.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DocuMind.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocuMindOptions(this IServiceCollection services, IConfiguration configuration)
    {
        // Strongly-typed options bindings will be added per feature slice (Identity, Azure, etc.).
        return services;
    }

    public static IServiceCollection AddDocuMindCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5173" };

        services.AddCors(options =>
        {
            options.AddPolicy("DocuMindFrontend", policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        return services;
    }

    public static IServiceCollection AddDocuMindAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt");
        var key = jwt["SigningKey"] ?? "dev-only-change-me-dev-only-change-me-32+chars";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt["Issuer"] ?? "documind",
                    ValidAudience = jwt["Audience"] ?? "documind",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddDocuMindHealthChecks(this IServiceCollection services, IConfiguration _)
    {
        services
            .AddHealthChecks()
            .AddDbContextCheck<DocuMindDbContext>(
                name: "database",
                tags: new[] { "ready", "db" });

        return services;
    }

    public static IServiceCollection AddDocuMindOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();
        return services;
    }
}
