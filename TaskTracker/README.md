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

All non-2xx responses use the **RFC 7807 ProblemDetails** format with `Content-Type: application/problem+json`. This includes 400 (validation errors come back as `ValidationProblemDetails` with a per-field `errors` map), 404 (not found, including unknown routes), and 500 (uncaught exceptions). Domain rule violations are translated to 400 ProblemDetails by `DomainExceptionHandler`.

## Tests

```bash
dotnet test
```

Single test:

```bash
dotnet test --filter "FullyQualifiedName~MyTestName"
```

Two-level pyramid: **domain unit tests** for entity invariants and **API integration tests** (`WebApplicationFactory<Program>` + SQLite `:memory:`) for the full HTTP slice. Application handlers are intentionally not unit-tested — they're pass-through orchestration today, so integration tests cover them transitively. A separate Application unit-test project becomes worthwhile only when a handler grows real logic (branching, application-level rules, multi-repository orchestration).

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

## Deliberate non-choices

A few common .NET libraries are intentionally **not** part of this solution. They are good tools, but at this scale they add ceremony without paying for themselves:

- **Object-to-object mapper** (AutoMapper, Mapster, Mapperly). `TaskDto.FromEntity(...)` is a single explicit line — the compiler keeps it in sync with the entity, navigation is one click, and there's no reflection or DI registration to learn. A mapper starts to earn its keep when an aggregate has many fields and several DTO variants per use case.
- **Mediator library** (MediatR, Mediator). Endpoints inject handlers directly. We don't currently have cross-cutting concerns that would benefit from a pipeline of behaviours, and FastEndpoints already provides validators and pre/post processors at the HTTP edge.

Both decisions are easy to reverse later — if the project grows enough to need either, swapping them in is a localized change.
