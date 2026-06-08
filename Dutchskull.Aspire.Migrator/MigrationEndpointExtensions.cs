using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dutchskull.Aspire.Migrator;

public static class MigrationEndpointExtensions
{
    public static IEndpointRouteBuilder MapDevelopmentMigrationEndpoints<TContext>(
        this IEndpointRouteBuilder endpoints)
        where TContext : DbContext
    {
        return endpoints
            .MapDevelopmentMigrationEndpoints<TContext>(async (db, ct) => await db.Database.MigrateAsync(ct));
    }
    
    public static IEndpointRouteBuilder MapDevelopmentMigrationEndpoints<TContext>(
        this IEndpointRouteBuilder endpoints,
        Func<TContext, CancellationToken, Task> migrateAsync)
        where TContext : DbContext
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/migration");

        group.MapPost("/migrate", async (IServiceScopeFactory scopeFactory, CancellationToken cancellationToken) =>
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            TContext db = scope.ServiceProvider.GetRequiredService<TContext>();

            await migrateAsync(db, cancellationToken);
            return Results.Ok("Database migrated successfully.");
        });

        group.MapPost("/seed", async (IServiceScopeFactory scopeFactory) =>
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            TContext db = scope.ServiceProvider.GetRequiredService<TContext>();

            IEnumerable<IInitialSeeder<TContext>> seeders =
                scope.ServiceProvider.GetServices<IInitialSeeder<TContext>>();

            foreach (IInitialSeeder<TContext> seeder in seeders)
            {
                await seeder.SeedAsync(db);
            }

            return Results.Ok("Database seeded successfully.");
        });

        group.MapPost("/drop", async (IServiceScopeFactory scopeFactory, CancellationToken cancellationToken) =>
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            TContext db = scope.ServiceProvider.GetRequiredService<TContext>();

            await db.Database.EnsureDeletedAsync(cancellationToken);
            await migrateAsync(db, cancellationToken);

            return Results.Ok("Database wiped and schema recreated successfully.");
        });

        IConfiguration configuration = endpoints.ServiceProvider.GetRequiredService<IConfiguration>();

        if (!string.Equals(configuration["EF_MIGRATE_ON_START"], "true", StringComparison.OrdinalIgnoreCase))
        {
            return endpoints;
        }

        using IServiceScope scope = endpoints.ServiceProvider.CreateScope();
        TContext db = scope.ServiceProvider.GetRequiredService<TContext>();
        migrateAsync(db, CancellationToken.None).GetAwaiter().GetResult();

        if (!string.Equals(configuration["EF_SEED_ON_START"], "true", StringComparison.OrdinalIgnoreCase))
        {
            return endpoints;
        }

        foreach (IInitialSeeder<TContext> seeder in scope.ServiceProvider.GetServices<IInitialSeeder<TContext>>())
        {
            seeder.SeedAsync(db).GetAwaiter().GetResult();
        }

        return endpoints;
    }
}