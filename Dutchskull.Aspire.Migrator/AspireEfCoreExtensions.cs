using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Dutchskull.Aspire.Migrator;

public static class AspireEfCoreExtensions
{
    public static IResourceBuilder<ProjectResource> WithEfMigrationCommands(
        this IResourceBuilder<ProjectResource> builder,
        bool? autoMigrateOnStart = null,
        bool? autoSeedOnStart = null)
    {
        if (autoMigrateOnStart.HasValue)
            builder.WithEnvironment("EF_MIGRATE_ON_START", autoMigrateOnStart.Value.ToString());

        if (autoSeedOnStart.HasValue)
            builder.WithEnvironment("EF_SEED_ON_START", autoSeedOnStart.Value.ToString());

        builder.WithHttpCommand(
            "/api/migration/migrate",
            "EF Migrate",
            commandOptions: new HttpCommandOptions
            {
                Method = HttpMethod.Post,
                IconName = "DatabaseArrowUp",
                IconVariant = IconVariant.Filled,
                Description = "Runs pending Entity Framework migrations.",
                IsHighlighted = true
            });

        builder.WithHttpCommand(
            "/api/migration/drop",
            "EF Drop/Reset",
            commandOptions: new HttpCommandOptions
            {
                Method = HttpMethod.Post,
                IconName = "Delete",
                IconVariant = IconVariant.Filled,
                Description = "Deletes and re-creates the database schema.",
                ConfirmationMessage = "Are you sure you want to drop and recreate the database? This cannot be undone.",
                IsHighlighted = true
            });

        builder.WithHttpCommand(
            "/api/migration/seed",
            "EF Seed",
            commandOptions: new HttpCommandOptions
            {
                Method = HttpMethod.Post,
                IconName = "DatabaseLightning",
                IconVariant = IconVariant.Filled,
                Description = "Executes data seeding logic.",
                IsHighlighted = true
            });

        return builder;
    }
}