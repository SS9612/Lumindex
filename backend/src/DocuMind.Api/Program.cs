using DocuMind.Api.Extensions;
using DocuMind.Application;
using DocuMind.Infrastructure;
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

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseCors("DocuMindFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDocuMindHealthChecks();

app.Run();

public partial class Program;
