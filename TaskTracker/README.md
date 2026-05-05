# TaskTracker

Small .NET 10 Web API for a Task Tracker. The solution is a Clean Architecture scaffold meant to showcase modern .NET practices on a deliberately small surface.

## Stack

- .NET 10 (`net10.0`), nullable + implicit usings everywhere.
- ASP.NET Core minimal hosting + [FastEndpoints](https://fast-endpoints.com/) for the HTTP layer.
- OpenAPI via `FastEndpoints.Swagger`, browsed through [Scalar](https://github.com/scalar/scalar).
- Central Package Management (`Directory.Packages.props`).
- xUnit for unit & integration tests; integration tests use `Microsoft.AspNetCore.Mvc.Testing`.

## Layout

```
TaskTracker.Domain          domain entities, value objects, rules
TaskTracker.Application     use cases, contracts (repositories, services)
TaskTracker.Infrastructure  persistence, external integrations
TaskTracker.Api             composition root, HTTP endpoints

TaskTracker.Domain.UnitTests       fast unit tests on Domain rules
TaskTracker.Api.IntegrationTests   in-memory HTTP tests via WebApplicationFactory
```

Reference direction:

```
Api ──► Application ──► Domain
  └───► Infrastructure ──► Application ──► Domain
```

## Getting started

```bash
dotnet restore
dotnet build
dotnet run --project TaskTracker.Api --launch-profile http
```

API runs on `http://localhost:5108` (or `https://localhost:7062` with the `https` profile).

In Development mode:
- OpenAPI: `/swagger/v1/swagger.json`
- Scalar UI: `/scalar/v1`
- Health: `/health`

## Tests

```bash
dotnet test
```

Single test:

```bash
dotnet test --filter "FullyQualifiedName~MyTestName"
```
