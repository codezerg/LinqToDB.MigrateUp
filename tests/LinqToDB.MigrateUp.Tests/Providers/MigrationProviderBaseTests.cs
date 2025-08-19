using FluentAssertions;
using LinqToDB.MigrateUp.Providers;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.MigrateUp.Tests.Testing;
using LinqToDB.MigrateUp.Tests.Infrastructure;
using LinqToDB.MigrateUp.Tests.TestEntities;
using NUnit.Framework;

namespace LinqToDB.MigrateUp.Tests.Providers;

[TestFixture]
public class MigrationProviderBaseTests
{
    private TestDatabase _database = null!;
    private TestMigrationProvider _provider = null!;
    private MockMigrationStateManager _mockStateManager = null!;

    [SetUp]
    public void SetUp()
    {
        _database = new TestDatabase();
        var migration = _database.CreateMigration();
        var mockSchemaService = new MockDatabaseSchemaService();
        var mockMutationService = new MockDatabaseMutationService();
        _mockStateManager = new MockMigrationStateManager();
        _provider = new TestMigrationProvider(migration, mockSchemaService, mockMutationService, _mockStateManager);
    }

    [TearDown]
    public void TearDown()
    {
        _database?.Dispose();
    }

    [Test]
    public void GetEntityColumns_ReturnsCorrectColumns_ForEntity()
    {
        // Act
        var columns = _provider.GetEntityColumns<Person>().ToList();

        // Assert
        columns.Should().NotBeEmpty();
        columns.Should().Contain(c => c.ColumnName == "PersonId");
        columns.Should().Contain(c => c.ColumnName == "FirstName");
        columns.Should().Contain(c => c.ColumnName == "LastName");
        columns.Should().Contain(c => c.ColumnName == "Age");
    }

    [Test]
    public void UpdateTableSchema_CreatesTable_WhenTableDoesNotExist()
    {
        // Arrange
        _provider.SetTableExists("Persons", false);

        // Act
        _provider.UpdateTableSchema<Person>();

        // Assert
        _provider.CreatedTables.Should().Contain("Persons");
        _mockStateManager.IsTableCreated("Persons").Should().BeTrue();
    }

    [Test]
    public void UpdateTableSchema_AddsColumns_WhenColumnsAreMissing()
    {
        // Arrange
        _provider.SetTableExists("Persons", true);
        _provider.SetTableColumns("Persons", new[]
        {
            new TableColumn("PersonId", "INTEGER", false),
            new TableColumn("FirstName", "TEXT", false)
            // Missing other columns
        });

        // Act
        _provider.UpdateTableSchema<Person>();

        // Assert
        _provider.CreatedColumns.Should().Contain(c => c.Key == "Persons" && c.Value.Any(col => col.ColumnName == "LastName"));
        _provider.CreatedColumns.Should().Contain(c => c.Key == "Persons" && c.Value.Any(col => col.ColumnName == "Age"));
    }

    [Test]
    public void UpdateTableSchema_AltersColumns_WhenColumnTypesAreDifferent()
    {
        // Arrange
        _provider.SetTableExists("Persons", true);
        _provider.SetTableColumns("Persons", new[]
        {
            new TableColumn("PersonId", "INTEGER", false),
            new TableColumn("FirstName", "VARCHAR(50)", false), // Wrong type
            new TableColumn("LastName", "TEXT", false),
            new TableColumn("Age", "TEXT", false) // Wrong type
        });

        // Act
        _provider.UpdateTableSchema<Person>();

        // Assert
        _provider.AlteredColumns.Should().NotBeEmpty();
    }

    [Test]
    public void EnsureIndex_CreatesIndex_WhenIndexDoesNotExist()
    {
        // Arrange
        var indexName = "IX_Test_Index";
        var columns = new[]
        {
            new TableIndexColumn("FirstName", true),
            new TableIndexColumn("LastName", false)
        };
        _provider.SetIndexExists("Persons", indexName, false);

        // Act
        _provider.EnsureIndex<Person>(indexName, columns);

        // Assert
        _provider.CreatedIndexes.Should().Contain(i => i.IndexName == indexName);
        _provider.CreatedIndexes.Should().Contain(i => i.Columns.Count() == 2);
    }

    [Test]
    public void EnsureIndex_ReplacesIndex_WhenColumnsDiffer()
    {
        // Arrange
        var indexName = "IX_Test_Index";
        var oldColumns = new[] { new TableIndexColumn("FirstName", true) };
        var newColumns = new[] { new TableIndexColumn("LastName", true) };
        
        _provider.SetIndexExists("Persons", indexName, true);
        _provider.SetIndexColumns("Persons", indexName, oldColumns);

        // Act
        _provider.EnsureIndex<Person>(indexName, newColumns);

        // Assert
        _provider.DroppedIndexes.Should().Contain(i => i.IndexName == indexName);
        _provider.CreatedIndexes.Should().Contain(i => i.IndexName == indexName);
    }

    [Test]
    public void EnsureIndex_ThrowsException_ForEmptyIndexName()
    {
        // Arrange
        var columns = new[] { new TableIndexColumn("FirstName", true) };

        // Act & Assert
        _provider.Invoking(p => p.EnsureIndex<Person>("", columns))
            .Should().Throw<ArgumentException>();
        
        _provider.Invoking(p => p.EnsureIndex<Person>(null!, columns))
            .Should().Throw<ArgumentException>();
    }

    [Test]
    public void EnsureIndex_ThrowsException_ForEmptyColumns()
    {
        // Act & Assert
        _provider.Invoking(p => p.EnsureIndex<Person>("IX_Test", null!))
            .Should().Throw<ArgumentException>();
        
        _provider.Invoking(p => p.EnsureIndex<Person>("IX_Test", Array.Empty<TableIndexColumn>()))
            .Should().Throw<ArgumentException>();
    }
}