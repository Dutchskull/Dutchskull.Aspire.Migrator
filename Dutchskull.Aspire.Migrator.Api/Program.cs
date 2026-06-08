using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<ApplicationDbContext>("migrator-db");

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", (ApplicationDbContext db) => db.Users.ToListAsync());

app.UseHttpsRedirection();

app.Run();

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}

public class User
{
    protected User()
    {
    }

    private User(string name)
    {
        Name = name;
    }

    public Guid Id { get; private init; }

    public string Name { get; private init; } = string.Empty;

    public static User Create(string name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("name required", nameof(name))
            : new User(name) { Id = Guid.NewGuid() };
    }
}