# Dutchskull.Aspire.Migrator

A .NET Aspire integration for managing Entity Framework Core database migrations and seeding through Aspire dashboard commands and automatic startup execution.

## Projects

### Dutchskull.Aspire.Migrator (shared library)

The core package. Provides:

- **`IInitialSeeder<TContext>`** — Interface for data seeders. Register multiple implementations for a `DbContext` and they run sequentially.

- **`MapDevelopmentMigrationEndpoints<TContext>()`** — Extension on `IEndpointRouteBuilder` that maps three endpoints under `/api/migration`:
  - `POST /migrate` — Applies pending migrations
  - `POST /seed` — Runs all registered `IInitialSeeder<TContext>` implementations
  - `POST /drop` — Drops and recreates the database schema

  Also reads `EF_MIGRATE_ON_START` and `EF_SEED_ON_START` from configuration. If enabled, runs migration (and optionally seeding) inline during startup.

- **`WithEfMigrationCommands()`** — Extension on `IResourceBuilder<ProjectResource>` for the AppHost. Registers dashboard command buttons (Migrate, Drop/Reset, Seed) and optionally sets `EF_MIGRATE_ON_START` / `EF_SEED_ON_START` environment variables.

### Dutchskull.Aspire.Migrator.Api

Your application API. Contains the `ApplicationDbContext` and entity models. Gets its connection string from Aspire's `"migrator-db"` resource.

### Dutchskull.Aspire.Migrator.DatabaseMigrator

A dedicated service that hosts the migration endpoints. References the API project for `DbContext` access. Registers seeders and calls `MapDevelopmentMigrationEndpoints`. This runs as a separate Aspire resource alongside your API.

### Dutchskull.Aspire.Migrator.Apphost

The Aspire orchestrator. Sets up PostgreSQL, adds the API and DatabaseMigrator projects, and wires them together with `WithEfMigrationCommands`.

## Usage

### 1. AppHost

```csharp
IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> db = builder
    .AddPostgres("postgres")
    .AddDatabase("migrator-db");

IResourceBuilder<ProjectResource> migrator = builder
    .AddProject<Dutchskull_Aspire_Migrator_DatabaseMigrator>("migrator")
    .WithReference(db)
    .WaitFor(db)
    .WithEfMigrationCommands(autoMigrateOnStart: true, autoSeedOnStart: true);

builder.AddProject<Dutchskull_Aspire_Migrator_Api>("api")
    .WaitFor(migrator)
    .WithReference(db);

builder.Build().Run();
```

The `WithEfMigrationCommands` call registers Aspire dashboard command buttons and sets the `EF_MIGRATE_ON_START` / `EF_SEED_ON_START` environment variables on the migrator project.

### 2. DatabaseMigrator

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IInitialSeeder<ApplicationDbContext>, UserSeeder>();
builder.AddNpgsqlDbContext<ApplicationDbContext>("migrator-db");

WebApplication app = builder.Build();

app.MapDevelopmentMigrationEndpoints<ApplicationDbContext>(
    async (db, ct) => await db.Database.MigrateAsync(ct));

app.Run();
```

The extension method reads the environment variables set by the AppHost and runs migration/seed on startup if enabled.

### 3. Implementing a Seeder

```csharp
public class UserSeeder : IInitialSeeder<ApplicationDbContext>
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        await context.Users.AddRangeAsync(
            User.Create("John"),
            User.Create("Travolta"),
            User.Create("The dude"));

        await context.SaveChangesAsync();
    }
}
```

## Environment Variables

| Variable | Values | Description |
|---|---|---|
| `EF_MIGRATE_ON_START` | `true` / `false` | Run migrations when the migrator starts |
| `EF_SEED_ON_START` | `true` / `false` | Run seeders after migration on startup |

Set via `WithEfMigrationCommands(autoMigrateOnStart: true, autoSeedOnStart: true)` or directly with `.WithEnvironment("EF_MIGRATE_ON_START", "true")`.

## Dependencies

- .NET 10
- `Aspire.Hosting` (≥ 13.4.2)
- `Microsoft.EntityFrameworkCore` (≥ 10.0.8)
