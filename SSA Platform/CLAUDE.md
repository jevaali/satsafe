# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All commands run from the solution root unless noted otherwise.

```bash
# Build
dotnet build

# Run the web app
dotnet run --project WebApp

# EF migrations (run from solution root)
dotnet ef migrations --project App.Infrastructure --startup-project WebApp add <MigrationName>
dotnet ef database   --project App.Infrastructure --startup-project WebApp update
dotnet ef database   --project App.Infrastructure --startup-project WebApp drop
dotnet ef migrations --project App.Infrastructure --startup-project WebApp remove
```

### Tooling

```bash
dotnet tool update -g dotnet-ef
dotnet tool update -g dotnet-aspnet-codegenerator
dotnet tool update -g Microsoft.Web.LibraryManager.Cli
```

### JS libs (run from WebApp directory)

```bash
libman install htmx.org --files dist/htmx.min.js
libman install alpinejs --files dist/cdn.min.js
```

### Scaffolding controllers (run from WebApp directory)

```bash
# MVC controller (disable global warnings-as-errors before running)
dotnet aspnet-codegenerator controller -name <Name>Controller -m <ModelName> -actions -dc AppDbContext -outDir Areas/<Area>/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f

# API controller
dotnet aspnet-codegenerator controller -name <Name>Controller -m <ModelName> -actions -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
```

## Architecture

This is an ASP.NET Core 10 MVC application using SQLite (dev) with a clean layered architecture.

### Project layers

| Project | Role |
|---|---|
| `Base.Contracts` | Interfaces for base entity types (meta, soft-delete, etc.) |
| `Base.Domain` | Concrete base entity classes (`BaseEntity`, `BaseEntityWithMeta`, `BaseEntityWithMetaSoftDelete`) |
| `Base.Helpers` | Identity helper utilities |
| `App.Domain` | Domain entities, value objects, enums, and `ConjunctionAnalysisService` |
| `App.Application` | Use cases, DTOs, and repository/unit-of-work interfaces |
| `App.Infrastructure` | EF Core `AppDbContext`, repository implementations, seeding, migrations |
| `WebApp` | ASP.NET MVC + Razor Pages, organized by Areas |

### Domain model

- `SpaceObject` (abstract, TPH discriminator) → `Satellite` / `DebrisObject`
- `Satellite` belongs to an `Operator` via `OperatorId`; has `Orbit` (1:1) and `TleRecords` (1:many)
- `ConjunctionEvent` links two `SpaceObject` entities (primary/secondary) and has a `RiskAssessment` (1:1)
- `UserOperator` join table links `AppUser` ↔ `Operator` (many-to-many)
- `Alert` belongs to a `ConjunctionEvent` and `AppUser`
- `LangStr` value object is stored as JSON in the database (used for multi-language fields like `Name`, `Country`, `Message`)
- `OrbitParameters` is an EF owned type stored as flat columns on `Orbit`

### Identity & authorization

- Custom `AppUser : IdentityUser<Guid>` and `AppRole`
- Authorization uses two layers: `[Authorize]` attribute (authentication) + manual role check via `User.IsInRole("Operator")` + `UserOperator` table lookup
- Default seed user: `operator@ssa.com`

### Web app structure

Areas:
- `Admin` — full CRUD for all entities
- `Operator` — operator-facing dashboard (satellites, conjunctions, alerts, settings)
- `Identity` — scaffolded ASP.NET Identity UI

### Conjunction screening

`ConjunctionAnalysisService` (in `App.Domain.Services`) performs orbital propagation using Keplerian mechanics over a 24-hour window at 60-second time steps, with a default threshold of 5 km. `RunConjunctionScreeningUseCase` invokes this service, deduplicates against existing events, persists new `ConjunctionEvent` records, and creates `Alert` entities for high-risk events.

### Database seeding

`AppDataInit.SeedAppData` loads satellites and debris from JSON files (`satellites.json`, `debris.json`) using `JsonSeeder`, then calls `SeedConjunctionTestData` to set up test operator/user/satellite data.
