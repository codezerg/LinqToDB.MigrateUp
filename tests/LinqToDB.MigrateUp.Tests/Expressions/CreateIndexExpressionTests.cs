using FluentAssertions;
using LinqToDB.MigrateUp.Expressions;
using LinqToDB.MigrateUp.Tests.Infrastructure;
using LinqToDB.MigrateUp.Tests.Providers;
using LinqToDB.MigrateUp.Tests.TestEntities;
using NUnit.Framework;

namespace LinqToDB.MigrateUp.Tests.Expressions;

[TestFixture]
public class CreateIndexExpressionTests
{
    private TestDatabase _database = null!;
    private TestMigrationProvider _provider = null!;
    private Migration _migration = null!;

    [SetUp]
    public void SetUp()
    {
        _database = new TestDatabase();
        using var connection = _database.CreateConnection();
        _migration = new Migration(connection);
        _provider = new TestMigrationProvider(_migration);
    }

    [TearDown]
    public void TearDown()
    {
        _database?.Dispose();
    }

    [Test]
    public void CreateIndexExpression_HasCorrectEntityType()
    {
        // Arrange & Act
        var expression = new CreateIndexExpression<Person>(new TestMigrationProfile());

        // Assert
        expression.EntityType.Should().Be(typeof(Person));
    }

    [Test]
    public void HasName_SetsProvidedIndexName()
    {
        // Arrange
        var expression = new CreateIndexExpression<Person>(new TestMigrationProfile());

        // Act
        expression.HasName("IX_Custom_Index");

        // Assert
        expression.ProvidedIndexName.Should().Be("IX_Custom_Index");
    }

    [Test]
    public void AddColumn_ByName_AddsColumnToList()
    {
        // Arrange
        var expression = new CreateIndexExpression<Person>(new TestMigrationProfile());

        // Act
        expression.AddColumn("FirstName", ascending: true);
        expression.AddColumn("LastName", ascending: false);

        // Assert
        expression.Columns.Should().HaveCount(2);
        expression.Columns[0].ColumnName.Should().Be("FirstName");
        expression.Columns[0].Ascending.Should().BeTrue();
        expression.Columns[1].ColumnName.Should().Be("LastName");
        expression.Columns[1].Ascending.Should().BeFalse();
    }

    [Test]
    public void AddColumn_ByExpression_AddsColumnToList()
    {
        // Arrange
        var expression = new CreateIndexExpression<Person>(new TestMigrationProfile());

        // Act
        expression.AddColumn(x => x.FirstName, ascending: true);
        expression.AddColumn(x => x.Age, ascending: false);

        // Assert
        expression.Columns.Should().HaveCount(2);
        expression.Columns[0].ColumnName.Should().Be("FirstName");
        expression.Columns[0].Ascending.Should().BeTrue();
        expression.Columns[1].ColumnName.Should().Be("Age");
        expression.Columns[1].Ascending.Should().BeFalse();
    }

    [Test]
    public void AddColumn_ByExpression_ThrowsException_ForInvalidExpression()
    {
        // Arrange
        var expression = new CreateIndexExpression<Person>(new TestMigrationProfile());

        // Act & Assert
        expression.Invoking(e => e.AddColumn(x => x.FirstName.Length))
            .Should().Throw<ArgumentException>()
            .WithParameterName("columnSelector");
    }

    [Test]
    public void Run_GeneratesIndexName_WhenNotProvided()
    {
        // Arrange
        var expression = new CreateIndexExpression<Person>(new TestMigrationProfile());
        expression.AddColumn(x => x.FirstName);
        expression.AddColumn(x => x.LastName);
        _provider.SetIndexExists("Persons", "IX_Persons_FirstName_LastName", false);

        // Act
        ((IMigrationTask)expression).Run(_provider);

        // Assert
        _provider.CreatedIndexes.Should().Contain(i => i.IndexName == "IX_Persons_FirstName_LastName");
    }

    [Test]
    public void Run_UsesProvidedIndexName_WhenSpecified()
    {
        // Arrange
        var expression = new CreateIndexExpression<Person>(new TestMigrationProfile());
        expression.HasName("IX_Custom_Name")
                 .AddColumn(x => x.FirstName);
        _provider.SetIndexExists("Persons", "IX_Custom_Name", false);

        // Act
        ((IMigrationTask)expression).Run(_provider);

        // Assert
        _provider.CreatedIndexes.Should().Contain(i => i.IndexName == "IX_Custom_Name");
    }

    [Test]
    public void Run_SkipsCreation_WhenIndexAlreadyExists()
    {
        // Arrange
        var expression = new CreateIndexExpression<Person>(new TestMigrationProfile());
        expression.AddColumn(x => x.FirstName);
        var indexName = "IX_Persons_FirstName";
        
        _provider.SetIndexExists("Persons", indexName, false);

        // Act - Run twice
        ((IMigrationTask)expression).Run(_provider);
        ((IMigrationTask)expression).Run(_provider);

        // Assert
        _provider.CreatedIndexes.Should().ContainSingle(i => i.IndexName == indexName);
        _migration.IndexesCreated.Should().Contain("Persons:IX_Persons_FirstName");
    }

    [Test]
    public void Run_ThrowsException_WhenNoColumnsSpecified()
    {
        // Arrange
        var expression = new CreateIndexExpression<Person>(new TestMigrationProfile());

        // Act & Assert
        expression.Invoking(e => ((IMigrationTask)e).Run(_provider))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("At least one column must be specified for the index.");
    }

    [Test]
    public void Run_CallsEnsureIndex_WithCorrectParameters()
    {
        // Arrange
        var expression = new CreateIndexExpression<Person>(new TestMigrationProfile());
        expression.HasName("IX_Test")
                 .AddColumn(x => x.FirstName, true)
                 .AddColumn(x => x.LastName, false);
        
        _provider.SetIndexExists("Persons", "IX_Test", false);

        // Act
        ((IMigrationTask)expression).Run(_provider);

        // Assert
        var createdIndex = _provider.CreatedIndexes.Should().ContainSingle().Subject;
        createdIndex.TableName.Should().Be("Persons");
        createdIndex.IndexName.Should().Be("IX_Test");
        createdIndex.Columns.Should().HaveCount(2);
        createdIndex.Columns.First().ColumnName.Should().Be("FirstName");
        createdIndex.Columns.First().Ascending.Should().BeTrue();
        createdIndex.Columns.Last().ColumnName.Should().Be("LastName");
        createdIndex.Columns.Last().Ascending.Should().BeFalse();
    }

    private class TestMigrationProfile : MigrationProfile
    {
        // Empty profile for testing
    }
}