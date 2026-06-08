using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dutchskull.Aspire.Migrator;

public static class MaintenanceEndpointExtensions
{
    public static IEndpointRouteBuilder MapDevelopmentMaintenanceEndpoints<TContext>(
        this IEndpointRouteBuilder endpoints,
        Func<TContext, CancellationToken, Task> migrateAsync)
        where TContext : DbContext
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/maintenance");

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

        return endpoints;
    }
}