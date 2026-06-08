using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Dutchskull.Aspire.Migrator;

public static class AspireEfCoreExtensions
{
    public static IResourceBuilder<ProjectResource> WithEfMaintenanceCommands(
        this IResourceBuilder<ProjectResource> builder)
    {
        builder.WithHttpCommand(
            "/api/maintenance/migrate",
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
            "/api/maintenance/drop",
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
            "/api/maintenance/seed",
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