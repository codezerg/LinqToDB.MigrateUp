using FluentAssertions;
using LinqToDB.MigrateUp.Expressions;
using LinqToDB.MigrateUp.Tests.Infrastructure;
using LinqToDB.MigrateUp.Tests.Providers;
using LinqToDB.MigrateUp.Tests.TestEntities;
using NUnit.Framework;

namespace LinqToDB.MigrateUp.Tests.Expressions;

[TestFixture]
public class CreateTableExpressionTests
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
    public void CreateTableExpression_HasCorrectEntityType()
    {
        // Arrange & Act
        var expression = new CreateTableExpression<Person>(new TestMigrationProfile());

        // Assert
        expression.EntityType.Should().Be(typeof(Person));
    }

    [Test]
    public void Run_CallsUpdateTableSchema_OnProvider()
    {
        // Arrange
        var profile = new TestMigrationProfile();
        var expression = new CreateTableExpression<Person>(profile);
        _provider.SetTableExists("Persons", false);

        // Act
        ((IMigrationTask)expression).Run(_provider);

        // Assert
        _provider.CreatedTables.Should().Contain("Persons");
    }

    [Test]
    public void Run_ThrowsArgumentNullException_WhenProviderIsNull()
    {
        // Arrange
        var profile = new TestMigrationProfile();
        var expression = new CreateTableExpression<Person>(profile);

        // Act & Assert
        expression.Invoking(e => ((IMigrationTask)e).Run(null!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("provider");
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenProfileIsNull()
    {
        // Act & Assert
        var action = () => new CreateTableExpression<Person>(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("profile");
    }

    [Test]
    public void Profile_ReturnsCorrectProfile()
    {
        // Arrange
        var profile = new TestMigrationProfile();
        
        // Act
        var expression = new CreateTableExpression<Person>(profile);

        // Assert
        expression.Profile.Should().Be(profile);
    }

    private class TestMigrationProfile : MigrationProfile
    {
        // Empty profile for testing
    }
}