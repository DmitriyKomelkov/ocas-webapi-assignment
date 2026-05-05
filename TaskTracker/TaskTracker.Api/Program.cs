using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TaskTracker.Api.Features.Tasks.Common;
using TaskTracker.Application;
using TaskTracker.Infrastructure;
using TaskTracker.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- Application & Infrastructure layers ---
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// --- Cross-cutting services ---
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddHealthChecks();

builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestMethod
                    | HttpLoggingFields.RequestPath
                    | HttpLoggingFields.ResponseStatusCode
                    | HttpLoggingFields.Duration;
    o.CombineLogs = true;
});

// --- JSON serialization (enums as strings on the wire) ---
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// --- HTTP layer: FastEndpoints + OpenAPI ---
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.DocumentName = "v1";
        s.Title = "Task Tracker API";
        s.Version = "v1";
        s.Description = "Small .NET Web API for a Task Tracker";
    };
    o.ShortSchemaNames = true;
    o.AutoTagPathSegmentIndex = 0;
});

var app = builder.Build();

// --- Startup tasks ---
await ApplyMigrationsAsync(app);

// --- Middleware pipeline ---
// UseExceptionHandler turns uncaught exceptions into RFC 7807 ProblemDetails (DomainException
// is handled separately by DomainExceptionHandler). UseStatusCodePages converts empty-body
// status responses (e.g. 404 from Send.NotFoundAsync()) into ProblemDetails as well.
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseFastEndpoints(c =>
{
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
    c.Errors.UseProblemDetails();
});

app.MapHealthChecks("/health");

// --- Development-only: Swagger JSON + Scalar UI + root redirect ---
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
    app.MapScalarApiReference(options =>
    {
        options.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json";
        options.Title = "Task Tracker API Reference";
        options.Theme = ScalarTheme.Moon;
        options.Layout = ScalarLayout.Modern;
    });

    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.Run();

// --- Local helpers ---
static async Task ApplyMigrationsAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
    await db.Database.MigrateAsync();
}

// Required by WebApplicationFactory<Program> in TaskTracker.Api.IntegrationTests
// to expose the auto-generated entry-point class as public.
public partial class Program;
