# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

Solution file is `TaskTracker.slnx` (the new XML solution format). All `dotnet` commands accept it directly.

- Restore: `dotnet restore TaskTracker.slnx`
- Build: `dotnet build TaskTracker.slnx`
- Run the API (Development, http profile on http://localhost:5108): `dotnet run --project TaskTracker.Api --launch-profile http`
  - https profile (https://localhost:7062): `dotnet run --project TaskTracker.Api --launch-profile https`
- Run all tests: `dotnet test TaskTracker.slnx`
- Run a single test: `dotnet test --filter "FullyQualifiedName~<TestName>"`

When the API is running:
- Health: `/health`
- Ping (FastEndpoints smoke endpoint): `/ping`
- Tasks CRUD: `POST/GET /tasks`, `GET/PUT/DELETE /tasks/{id}`

In Development:
- Swagger JSON: `/swagger/v1/swagger.json`
- Scalar API reference UI: `/scalar/v1`

### EF Core / migrations

`dotnet-ef` is pinned as a **local** tool in `dotnet-tools.json`. Never `dotnet tool install -g`; use the local one — versions stay in sync with the EF runtime packages.

```bash
dotnet tool restore                                # first time on a fresh checkout
dotnet tool run dotnet-ef migrations add <Name> \
    --project TaskTracker.Infrastructure \
    --startup-project TaskTracker.Api \
    --output-dir Persistence/Migrations
```

Migrations are applied automatically at startup (`db.Database.MigrateAsync()` in `Program.cs`). The dev SQLite file (`tasktracker.db`) lives next to the Api project and is gitignored.

## Architecture

Clean architecture across six projects (four production + two test). Reference direction is enforced by project references — never reverse them:

```
Api ──► Application ──► Domain
  └───► Infrastructure ──► Application ──► Domain

Domain.UnitTests       ──► Domain
Api.IntegrationTests   ──► Api  (in-memory via WebApplicationFactory<Program> + SQLite ":memory:")
```

- `TaskTracker.Domain` — entities, value objects, domain rules, `DomainException`. **No outward references.**
- `TaskTracker.Application` — use cases + contracts (e.g. `ITaskRepository`) that Infrastructure must satisfy. Per-operation handlers (CQRS-lite). Exposes `AddApplication()` on `IServiceCollection`.
- `TaskTracker.Infrastructure` — `TaskTrackerDbContext`, EF Core mappings, repository implementations. Exposes `AddInfrastructure(IConfiguration)`.
- `TaskTracker.Api` — composition root. Wires `AddApplication()` + `AddInfrastructure(...)`. Hosts the HTTP server.
- `TaskTracker.Domain.UnitTests` — fast unit tests on Domain rules.
- `TaskTracker.Api.IntegrationTests` — `WebApplicationFactory<Program>`-based tests; `TaskTrackerWebApplicationFactory` swaps the DbContext for SQLite `:memory:`.

### Domain conventions

- Entities have **private setters** and mutate only through methods that enforce invariants (e.g. `TaskItem.Update`, `TaskItem.ChangeStatus`). Don't expose public setters or anaemic models — invariants belong on the entity.
- Validation that's a domain rule (e.g. "cannot be Done with empty title") lives in the entity and throws `DomainException`. Validation that's about request shape (max length, required) is duplicated in FE/FluentValidation at the API edge — defense in depth.
- Construct entities through static factories (`TaskItem.Create(...)`), not constructors.

### Application layer conventions

- One handler class per use case, in `Tasks/<Feature>/<Verb>Handler.cs`. Handlers are scoped, take `ITaskRepository` and any other contracts via ctor.
- Commands/queries are `sealed record`s in the same folder.
- Application-layer DTOs (`TaskDto`) are returned to API; API does not see Domain entities.
- `ITaskRepository` exposes a small set of operations (`GetByIdAsync`, `ListAsync`, `AddAsync`, `Remove`, `SaveChangesAsync`) — no `IQueryable<T>` leakage.

### Infrastructure conventions

- `IEntityTypeConfiguration<T>` per entity in `Persistence/Configurations/`. `OnModelCreating` calls `ApplyConfigurationsFromAssembly`.
- Enums are persisted as **strings** (`HasConversion<string>()`) — keeps the database readable and resilient to enum reordering.
- EF migrations in `Persistence/Migrations/`. The DB file path comes from `ConnectionStrings:TaskTracker` in configuration.

### API layer conventions

- **FastEndpoints** is the HTTP layer (not MVC controllers, not raw Minimal API). Auto-discovered via `AddFastEndpoints()` / `UseFastEndpoints()` in `Program.cs`. The endpoint API uses `Send.OkAsync(...)` / `Send.CreatedAtAsync<T>(...)` / `Send.NotFoundAsync()` (FE v7), not the older `SendAsync(...)`.
- Endpoints live under `TaskTracker.Api/Features/<FeatureName>/<Verb>/...` (vertical-slice). Each verb folder typically has `Endpoint.cs`, `Request.cs`, `Validator.cs`. See `Features/Tasks/Create/` for the canonical pattern.
- **FluentValidation** validators inherit from `Validator<TRequest>` and are auto-bound to the matching endpoint by FastEndpoints.
- Enums travel as strings on the wire — `JsonStringEnumConverter` is added to both `ConfigureHttpJsonOptions` and `UseFastEndpoints(c => c.Serializer.Options...)`.
- Exception → ProblemDetails mapping: `DomainException` is translated to 400 by `Features/Tasks/Common/DomainExceptionHandler` (registered via `AddExceptionHandler<T>`). Anything else uncaught → 500 ProblemDetails through the default `UseExceptionHandler()` pipeline.
- 404 from endpoints uses `Send.NotFoundAsync()`; `UseStatusCodePages()` then converts the empty body into ProblemDetails. Don't write 404 ProblemDetails by hand.
- `Program.cs` ends with `public partial class Program;` — required so `WebApplicationFactory<Program>` can see the entry point. Don't remove it.
- `TimeProvider.System` is registered as a singleton — inject `TimeProvider` instead of using `DateTime.UtcNow` in domain/application code so tests can swap in `FakeTimeProvider`.
- Middleware order: `UseExceptionHandler` → `UseStatusCodePages` → `UseHttpsRedirection` → `UseFastEndpoints` → `MapHealthChecks`. Don't reorder unless you know why.

### Integration tests pattern

`TaskTrackerWebApplicationFactory` overrides the registered `DbContextOptions<TaskTrackerDbContext>` to use a single shared `SqliteConnection("DataSource=:memory:")` per fixture. Migrations run on startup against this in-memory DB, so each test class gets a fresh schema. Use `IClassFixture<TaskTrackerWebApplicationFactory>` to share the factory across tests in a class; create separate test classes when state must be isolated.

### Build-wide settings

- `Directory.Build.props` centralizes `TargetFramework=net10.0`, `LangVersion=latest`, `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`, `AnalysisMode=All`. Individual `.csproj` files **must not duplicate these properties**.
- `Directory.Packages.props` — Central Package Management is on (`ManagePackageVersionsCentrally=true`) **plus** transitive pinning (`CentralPackageTransitivePinningEnabled=true`). Add new dependencies as `<PackageVersion ... />` here, then reference from `.csproj` with `<PackageReference Include="..." />` (no `Version` attribute).
- `global.json` pins SDK to `10.0.203` with `rollForward=latestFeature`.
- `.editorconfig` is detailed and authoritative — Allman braces, `var` for built-ins, no `this.` qualification, `_camelCase` private fields, `s_` static prefix, etc. Follow it rather than guessing.
- Test projects suppress `CA1707` via `<NoWarn>` to allow xUnit's `Method_Scenario_Result` naming with underscores.
- `NuGetAuditSuppress` in `Directory.Packages.props` covers two `System.Security.Cryptography.Xml` advisories that arrive transitively through EF.Design. The package is design-time-only (`PrivateAssets=all`), never shipped at runtime, so the advisories are not actually exploitable here. Re-evaluate once a patched 10.x is published.

### Adding a new feature (typical path)

1. **Domain** — entity + invariants in methods, throw `DomainException` on rule violations.
2. **Application** — define handler(s) under `<Aggregate>/<Verb>/`, command/query records, return DTOs. Register handlers in `AddApplication()`.
3. **Infrastructure** — extend the DbContext (`DbSet<T>`), add `IEntityTypeConfiguration<T>`, implement repository methods. Generate migration via `dotnet tool run dotnet-ef migrations add <Name> -p TaskTracker.Infrastructure -s TaskTracker.Api -o Persistence/Migrations`.
4. **Api** — add endpoint + validator under `Features/<Aggregate>/<Verb>/`. Translate domain exceptions in `Features/<Aggregate>/Common/` if a custom mapping is needed.
5. **Tests** — unit tests against Domain (and any non-trivial handler) + integration tests against the endpoint via `WebApplicationFactory<Program>`.
6. **Postman** — add the new request to `postman/TaskTracker.postman_collection.json` so the manual smoke flow stays current.
