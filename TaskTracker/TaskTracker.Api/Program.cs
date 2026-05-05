using FastEndpoints;
using FastEndpoints.Swagger;
using Scalar.AspNetCore;
using TaskTracker.Application;
using TaskTracker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

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
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseFastEndpoints();

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

public partial class Program;
