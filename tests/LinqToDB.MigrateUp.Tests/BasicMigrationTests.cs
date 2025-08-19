using FluentAssertions;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.MigrateUp.Tests.Infrastructure;
using LinqToDB.MigrateUp.Tests.TestEntities;
using LinqToDB.MigrateUp.Tests.TestProfiles;
using NUnit.Framework;

namespace LinqToDB.MigrateUp.Tests;

[TestFixture]
public class BasicMigrationTests
{
    private TestDatabase _database = null!;
    private TestMigrationLogger _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _database = new TestDatabase();
        _logger = new TestMigrationLogger();
    }

    [TearDown]
    public void TearDown()
    {
        _database?.Dispose();
    }

    [Test]
    public void Migration_CanRunSuccessfully()
    {
        // Arrange
        var migration = _database.CreateMigration(logger: _logger);
        var configuration = new MigrationConfiguration(config =>
        {
            config.AddProfile(new PersonMigrationProfile());
        });

        // Act & Assert - Should not throw
        migration.Invoking(m => m.Run(configuration))
            .Should().NotThrow();
    }

    [Test]
    public void Migration_WithCaching_CanRunTwice()
    {
        // Arrange
        var options = new MigrationOptions
        {
            EnableCaching = true,
            SkipCachedMigrations = true
        };
        var migration = _database.CreateMigration(options, _logger);
        var configuration = new MigrationConfiguration(config =>
        {
            config.AddProfile(new PersonMigrationProfile());
        });

        // Act & Assert - Should not throw when run multiple times
        migration.Invoking(m => m.Run(configuration)).Should().NotThrow();
        migration.Invoking(m => m.Run(configuration)).Should().NotThrow();
    }

    [Test]
    public void RunForEntity_WorksWithSpecificEntity()
    {
        // Arrange
        var migration = _database.CreateMigration(logger: _logger);
        var configuration = new MigrationConfiguration(config =>
        {
            config.AddProfile(new PersonMigrationProfile());
            config.AddProfile(new ProductMigrationProfile());
        });

        // Act & Assert - Should not throw
        migration.Invoking(m => m.RunForEntity<Person>(configuration))
            .Should().NotThrow();
    }

    [Test]
    public void MigrationOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new MigrationOptions();

        // Assert
        options.EnableCaching.Should().BeTrue();
        options.SkipCachedMigrations.Should().BeTrue();
        options.Cache.Should().NotBeNull();
    }

    [Test]
    public void Migration_TracksCreatedTables()
    {
        // Arrange
        var migration = _database.CreateMigration(logger: _logger);
        var configuration = new MigrationConfiguration(config =>
        {
            config.AddProfile(new PersonMigrationProfile());
        });

        // Act
        migration.Run(configuration);

        // Assert
        migration.TablesCreated.Should().NotBeEmpty();
    }

    [Test]
    public void Migration_TracksCreatedIndexes()
    {
        // Arrange
        var migration = _database.CreateMigration();
        var configuration = new MigrationConfiguration(config =>
        {
            config.AddProfile(new PersonMigrationProfile());
        });

        // Act
        migration.Run(configuration);

        // Assert
        migration.IndexesCreated.Should().NotBeEmpty();
    }

    private static bool TableExists(DataConnection connection, string tableName)
    {
        try
        {
            var query = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
            using var command = connection.CreateCommand();
            command.CommandText = query;
            var result = command.ExecuteScalar();
            return result != null && !string.IsNullOrEmpty(result.ToString());
        }
        catch
        {
            return false;
        }
    }
}