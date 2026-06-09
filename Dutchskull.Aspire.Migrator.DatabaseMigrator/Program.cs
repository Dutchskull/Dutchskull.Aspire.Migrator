using Dutchskull.Aspire.Migrator;
using Dutchskull.Aspire.Migrator.DatabaseMigrator;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddTransient<IInitialSeeder<ApplicationDbContext>, UserSeeder>();

builder.AddNpgsqlDbContext<ApplicationDbContext>("migrator-db");

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDevelopmentMigrationEndpoints<ApplicationDbContext>();

app.UseHttpsRedirection();

app.Run();