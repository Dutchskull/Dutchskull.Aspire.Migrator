using Dutchskull.Aspire.Migrator;
using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> db = builder
    .AddPostgres("postgres")
    .AddDatabase("migrator-db");

IResourceBuilder<ProjectResource> migrator = builder.AddProject<Dutchskull_Aspire_Migrator_DatabaseMigrator>("migrator")
    .WithReference(db)
    .WaitFor(db)
    .WithEfMigrationCommands(true, true);

builder.AddProject<Dutchskull_Aspire_Migrator_Api>("api")
    .WaitFor(migrator)
    .WithReference(db);

builder.Build().Run();