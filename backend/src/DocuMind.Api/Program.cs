using DocuMind.Api.Extensions;
using DocuMind.Application;
using DocuMind.Infrastructure;
using DocuMind.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services
    .AddDocuMindApplication()
    .AddDocuMindInfrastructure(builder.Configuration)
    .AddDocuMindOptions(builder.Configuration)
    .AddDocuMindCors(builder.Configuration)
    .AddDocuMindAuth(builder.Configuration)
    .AddDocuMindHealthChecks(builder.Configuration)
    .AddDocuMindOpenApi();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DocuMindDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Skip HTTPS redirection in Development: the Vite dev proxy talks to the HTTP endpoint, and a
// 307 redirect to the HTTPS port causes the browser to drop the Authorization header on the
// resulting cross-origin request. In production the platform terminates TLS in front of the app.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseCors("DocuMindFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDocuMindHealthChecks();

app.MapGet("/", () => Results.Ok(new
{
    name = "DocuMind API",
    status = "ok",
    environment = app.Environment.EnvironmentName,
    docs = "/openapi/v1.json",
    health = new { live = "/healthz", ready = "/readyz" },
}))
.ExcludeFromDescription();

app.Run();

public partial class Program;
