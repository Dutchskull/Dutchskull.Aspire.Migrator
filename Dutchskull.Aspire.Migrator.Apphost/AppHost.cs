using Dutchskull.Aspire.Migrator;
using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> db = builder
    .AddPostgres("postgres")
    .AddDatabase("migrator-db");

builder.AddProject<Dutchskull_Aspire_Migrator_Api>("api")
    .WithReference(db);

builder.AddProject<Dutchskull_Aspire_Migrator_DatabaseMigrator>("migrator")
    .WithReference(db)
    .WithEfMaintenanceCommands();

builder.Build().Run();