using System.Text;
using DocuMind.Api.Authentication;
using DocuMind.Api.Common;
using DocuMind.Application.Authentication;
using DocuMind.Application.Common.Interfaces;
using DocuMind.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DocuMind.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocuMindOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

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
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var key = string.IsNullOrWhiteSpace(jwt.SigningKey)
            ? "dev-only-change-me-dev-only-change-me-32+chars"
            : jwt.SigningKey;

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
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorization();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddExceptionHandler<ValidationExceptionHandler>();

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
