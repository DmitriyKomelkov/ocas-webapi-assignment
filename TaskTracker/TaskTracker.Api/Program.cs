using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TaskTracker.Api.Features.Tasks.Common;
using TaskTracker.Application;
using TaskTracker.Infrastructure;
using TaskTracker.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddHealthChecks();

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

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

await ApplyMigrationsAsync(app);

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseFastEndpoints(c =>
{
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
});

app.MapHealthChecks("/health");

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

static async Task ApplyMigrationsAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
    await db.Database.MigrateAsync();
}

public partial class Program;
