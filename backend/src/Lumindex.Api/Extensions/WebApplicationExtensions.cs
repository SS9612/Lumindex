using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Lumindex.Api.Extensions;

public static class WebApplicationExtensions
{
    public static IEndpointRouteBuilder MapLumindexHealthChecks(this IEndpointRouteBuilder endpoints)
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
