# TaskTracker

Small .NET 10 Web API for a Task Tracker. The solution is a Clean Architecture scaffold meant to showcase modern .NET practices on a deliberately small surface.

## Stack

- .NET 10 (`net10.0`), nullable + implicit usings everywhere.
- ASP.NET Core minimal hosting + [FastEndpoints](https://fast-endpoints.com/) for the HTTP layer.
- OpenAPI via `FastEndpoints.Swagger`, browsed through [Scalar](https://github.com/scalar/scalar).
- EF Core 10 + SQLite for persistence.
- FluentValidation for request validation.
- xUnit for unit & integration tests; integration tests use `Microsoft.AspNetCore.Mvc.Testing` + SQLite `:memory:`.
- Central Package Management (`Directory.Packages.props`) with transitive pinning.

## Layout

```
TaskTracker.Domain          entities, value objects, domain rules
TaskTracker.Application     use cases, contracts (repositories, handlers)
TaskTracker.Infrastructure  EF Core DbContext, persistence implementations
TaskTracker.Api             composition root, FastEndpoints HTTP layer

TaskTracker.Domain.UnitTests       fast unit tests on Domain rules
TaskTracker.Api.IntegrationTests   in-memory HTTP tests (WebApplicationFactory + SQLite ":memory:")
```

Reference direction:

```
Api ──► Application ──► Domain
  └───► Infrastructure ──► Application ──► Domain
```

## Getting started

```bash
dotnet tool restore       # one-time, restores the local dotnet-ef tool
dotnet restore
dotnet build
dotnet run --project TaskTracker.Api --launch-profile http
```

Migrations apply automatically on startup, so the SQLite file (`tasktracker.db` next to the Api project) is created on first run.

API runs on `http://localhost:5108` (or `https://localhost:7062` with the `https` profile). The dev launch profile opens the browser at the Scalar UI automatically.

### Endpoints

In Development:
- OpenAPI: `/swagger/v1/swagger.json`
- Scalar UI: `/scalar/v1`

Always available:
- `GET /health`
- `GET /ping`
- `POST /tasks`
- `GET /tasks`
- `GET /tasks/{id}`
- `PUT /tasks/{id}`
- `DELETE /tasks/{id}`

## Tests

```bash
dotnet test
```

Single test:

```bash
dotnet test --filter "FullyQualifiedName~MyTestName"
```

## Postman

A ready-to-use Postman collection is at `postman/TaskTracker.postman_collection.json`. It includes all CRUD endpoints, a few validation samples, and a `{{baseUrl}}` variable defaulting to `http://localhost:5108`. The "Create task" request stashes the new id into `{{taskId}}` so the next requests in the folder pick it up automatically.

## EF Core migrations

`dotnet-ef` is pinned as a local tool. To add a migration:

```bash
dotnet tool run dotnet-ef migrations add <Name> \
    --project TaskTracker.Infrastructure \
    --startup-project TaskTracker.Api \
    --output-dir Persistence/Migrations
```
