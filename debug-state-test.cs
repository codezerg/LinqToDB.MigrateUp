// Temporary debug test
using System;
using LinqToDB;
using LinqToDB.MigrateUp;
using LinqToDB.MigrateUp.Tests.Infrastructure;
using LinqToDB.MigrateUp.Tests.TestProfiles;

var database = new TestDatabase();
var migration = database.CreateMigration();
var configuration = new MigrationConfiguration(config =>
{
    config.AddProfile(new PersonMigrationProfile());
});

Console.WriteLine($"Before run - TablesCreated count: {migration.TablesCreated.Count}");
Console.WriteLine($"Before run - IndexesCreated count: {migration.IndexesCreated.Count}");

migration.Run(configuration);

Console.WriteLine($"After run - TablesCreated count: {migration.TablesCreated.Count}");
Console.WriteLine($"After run - IndexesCreated count: {migration.IndexesCreated.Count}");

foreach (var table in migration.TablesCreated)
{
    Console.WriteLine($"Created table: {table}");
}

foreach (var index in migration.IndexesCreated)
{
    Console.WriteLine($"Created index: {index}");
}

database.Dispose();