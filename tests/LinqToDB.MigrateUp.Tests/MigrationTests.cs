using FluentAssertions;
using LinqToDB.MigrateUp.Services;
using LinqToDB.MigrateUp.Tests.Testing;
using LinqToDB.MigrateUp.Tests.Infrastructure;
using LinqToDB.MigrateUp.Tests.TestEntities;
using LinqToDB.MigrateUp.Tests.TestProfiles;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace LinqToDB.MigrateUp.Tests;

[TestFixture]
public class MigrationTests
{
    private TestDatabase _database = null!;
    private TestLogger<Migration> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _database = new TestDatabase();
        _logger = new TestLogger<Migration>();
    }

    [TearDown]
    public void TearDown()
    {
        _database?.Dispose();
    }

    [Test]
    public void Run_ExecutesAllMigrations()
    {
        // Arrange
        using var connection = _database.CreateConnection();
        var dataService = new LinqToDbDataConnectionService(connection);
        var stateManager = new MigrationStateManager();
        var providerFactory = new DefaultMigrationProviderFactory();
        var migration = new Migration(dataService, stateManager, providerFactory, _logger);
        var configuration = new MigrationConfiguration(config =>
        {
            config.AddProfile(new PersonMigrationProfile());
            config.AddProfile(new ProductMigrationProfile());
        });

        // Act
        migration.Run(configuration);

        // Assert
        TableExists(connection, "Persons").Should().BeTrue();
        TableExists(connection, "Products").Should().BeTrue();
        _logger.InfoMessages.Should().NotBeEmpty();
    }

    [Test]
    public void RunForEntity_ExecutesOnlySpecificEntityMigrations()
    {
        // Arrange
        using var connection = _database.CreateConnection();
        var dataService = new LinqToDbDataConnectionService(connection);
        var stateManager = new MigrationStateManager();
        var providerFactory = new DefaultMigrationProviderFactory();
        var migration = new Migration(dataService, stateManager, providerFactory, _logger);
        var configuration = new MigrationConfiguration(config =>
        {
            config.AddProfile(new PersonMigrationProfile());
            config.AddProfile(new ProductMigrationProfile());
        });

        // Act
        migration.RunForEntity<Person>(configuration);

        // Assert
        TableExists(connection, "Persons").Should().BeTrue();
        TableExists(connection, "Products").Should().BeFalse();
        _logger.InfoMessages.Should().Contain(msg => msg.Contains("Person"));
        _logger.InfoMessages.Should().NotContain(msg => msg.Contains("Product"));
    }

    [Test]
    public void RunForEntity_WithCaching_SkipsCachedTasks()
    {
        // Arrange
        using var connection = _database.CreateConnection();
        var options = new MigrationOptions
        {
            EnableCaching = true,
            SkipCachedMigrations = true
        };
        var dataService = new LinqToDbDataConnectionService(connection);
        var stateManager = new MigrationStateManager();
        var providerFactory = new DefaultMigrationProviderFactory();
        var migration = new Migration(dataService, stateManager, providerFactory, _logger, options);
        var configuration = new MigrationConfiguration(config =>
        {
            config.AddProfile(new PersonMigrationProfile());
        });

        // Act - Run twice
        migration.RunForEntity<Person>(configuration);
        _logger.Reset();
        migration.RunForEntity<Person>(configuration);

        // Assert
        _logger.InfoMessages.Should().Contain(msg => msg.Contains("Skipping cached"));
    }

    [Test]
    public void RunForEntity_WithCachingDisabled_RunsTasksEveryTime()
    {
        // Arrange
        using var connection = _database.CreateConnection();
        var options = new MigrationOptions
        {
            EnableCaching = false
        };
        var dataService = new LinqToDbDataConnectionService(connection);
        var stateManager = new MigrationStateManager();
        var providerFactory = new DefaultMigrationProviderFactory();
        var migration = new Migration(dataService, stateManager, providerFactory, _logger, options);
        var configuration = new MigrationConfiguration(config =>
        {
            config.AddProfile(new PersonMigrationProfile());
        });

        // Act - Run twice
        migration.RunForEntity<Person>(configuration);
        var firstRunCount = _logger.InfoMessages.Count;
        _logger.Reset();
        migration.RunForEntity<Person>(configuration);

        // Assert
        _logger.InfoMessages.Should().NotContain(msg => msg.Contains("Skipping cached"));
        _logger.InfoMessages.Should().NotBeEmpty();
    }

    [Test]
    public void Run_WithMixedProfiles_ExecutesAllTasks()
    {
        // Arrange
        using var connection = _database.CreateConnection();
        var dataService = new LinqToDbDataConnectionService(connection);
        var stateManager = new MigrationStateManager();
        var providerFactory = new DefaultMigrationProviderFactory();
        var migration = new Migration(dataService, stateManager, providerFactory, _logger);
        var configuration = new MigrationConfiguration(config =>
        {
            config.AddProfile(new PersonMigrationProfile());
            config.AddProfile(new ProductMigrationProfile());
        });

        // Act
        migration.Run(configuration);

        // Assert
        TableExists(connection, "Persons").Should().BeTrue();
        TableExists(connection, "Products").Should().BeTrue();
        
        // Check that data was imported for Person (WhenTableCreated)
        var personCount = connection.GetTable<Person>().Count();
        personCount.Should().Be(2);
        
        // Products should be empty since they use WhenTableEmpty and table has data
        var productCount = connection.GetTable<Product>().Count();
        productCount.Should().Be(0);
    }

    [Test]
    public void Migration_TracksCreatedTablesAndIndexes()
    {
        // Arrange
        using var connection = _database.CreateConnection();
        var dataService = new LinqToDbDataConnectionService(connection);
        var stateManager = new MigrationStateManager();
        var providerFactory = new DefaultMigrationProviderFactory();
        var migration = new Migration(dataService, stateManager, providerFactory, Microsoft.Extensions.Logging.Abstractions.NullLogger<Migration>.Instance);
        var configuration = new MigrationConfiguration(config =>
        {
            config.AddProfile(new PersonMigrationProfile());
        });

        // Act
        migration.Run(configuration);

        // Assert
        migration.StateManager.IsTableCreated("Persons").Should().BeTrue();
        migration.StateManager.IsIndexCreated("IX_Persons_LastName").Should().BeTrue();
    }

    private static bool TableExists(LinqToDB.Data.DataConnection connection, string tableName)
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