using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace DocuMind.Api.Extensions;

public static class WebApplicationExtensions
{
    public static IEndpointRouteBuilder MapDocuMindHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = _ => false,
        });

        endpoints.MapHealthChecks("/readyz", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
        });

        return endpoints;
    }
}
