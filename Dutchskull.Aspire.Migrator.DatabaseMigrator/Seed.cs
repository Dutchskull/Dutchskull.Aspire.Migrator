using Microsoft.EntityFrameworkCore;

namespace Dutchskull.Aspire.Migrator.DatabaseMigrator;

public class UserSeeder : IInitialSeeder<ApplicationDbContext>
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        List<User> defaultProducts =
        [
            User.Create("John"),
            User.Create("Travolta"),
            User.Create("The dude")
        ];

        await context.Users.AddRangeAsync(defaultProducts);

        await context.SaveChangesAsync();
    }
}