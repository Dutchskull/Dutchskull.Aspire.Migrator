using Microsoft.EntityFrameworkCore;

namespace Dutchskull.Aspire.Migrator;

public interface IInitialSeeder<in TContext> where TContext : DbContext
{
    Task SeedAsync(TContext context);
}