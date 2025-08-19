using FluentAssertions;
using LinqToDB.MigrateUp.Tests.TestEntities;
using LinqToDB.MigrateUp.Tests.TestProfiles;
using NUnit.Framework;

namespace LinqToDB.MigrateUp.Tests;

[TestFixture]
public class MigrationProfileTests
{
    [Test]
    public void CreateTable_AddsTaskToProfile()
    {
        // Arrange
        var profile = new TestMigrationProfile();

        // Act
        profile.CreateTable<Person>();

        // Assert
        profile.Tasks.Should().HaveCount(1);
        profile.Tasks.First().EntityType.Should().Be(typeof(Person));
    }

    [Test]
    public void CreateIndex_AddsTaskToProfile()
    {
        // Arrange
        var profile = new TestMigrationProfile();

        // Act
        profile.CreateIndex<Person>()
               .AddColumn(x => x.FirstName);

        // Assert
        profile.Tasks.Should().HaveCount(1);
        profile.Tasks.First().EntityType.Should().Be(typeof(Person));
    }

    [Test]
    public void ImportData_AddsTaskToProfile()
    {
        // Arrange
        var profile = new TestMigrationProfile();

        // Act
        profile.ImportData<Person>()
               .Key(x => x.PersonId)
               .Source(() => new[] { new Person() });

        // Assert
        profile.Tasks.Should().HaveCount(1);
        profile.Tasks.First().EntityType.Should().Be(typeof(Person));
    }

    [Test]
    public void GetTasksForEntity_ReturnsOnlySpecificEntityTasks()
    {
        // Arrange
        var profile = new TestMigrationProfile();
        profile.CreateTable<Person>();
        profile.CreateTable<Product>();
        profile.CreateIndex<Person>().AddColumn(x => x.FirstName);

        // Act
        var personTasks = profile.GetTasksForEntity<Person>().ToList();

        // Assert
        personTasks.Should().HaveCount(2);
        personTasks.Should().OnlyContain(t => t.EntityType == typeof(Person));
    }

    [Test]
    public void GetTasksForEntity_ByType_ReturnsOnlySpecificEntityTasks()
    {
        // Arrange
        var profile = new TestMigrationProfile();
        profile.CreateTable<Person>();
        profile.CreateTable<Product>();
        profile.CreateIndex<Person>().AddColumn(x => x.FirstName);

        // Act
        var personTasks = profile.GetTasksForEntity(typeof(Person)).ToList();

        // Assert
        personTasks.Should().HaveCount(2);
        personTasks.Should().OnlyContain(t => t.EntityType == typeof(Person));
    }

    [Test]
    public void GetEntityTypes_ReturnsUniqueEntityTypes()
    {
        // Arrange
        var profile = new TestMigrationProfile();
        profile.CreateTable<Person>();
        profile.CreateTable<Product>();
        profile.CreateIndex<Person>().AddColumn(x => x.FirstName);
        profile.CreateIndex<Person>().AddColumn(x => x.LastName);

        // Act
        var entityTypes = profile.GetEntityTypes().ToList();

        // Assert
        entityTypes.Should().HaveCount(2);
        entityTypes.Should().Contain(typeof(Person));
        entityTypes.Should().Contain(typeof(Product));
    }

    [Test]
    public void GetTasksForEntity_ReturnsEmpty_WhenNoTasksForEntity()
    {
        // Arrange
        var profile = new TestMigrationProfile();
        profile.CreateTable<Person>();

        // Act
        var productTasks = profile.GetTasksForEntity<Product>().ToList();

        // Assert
        productTasks.Should().BeEmpty();
    }

    [Test]
    public void GetEntityTypes_ReturnsEmpty_WhenNoTasks()
    {
        // Arrange
        var profile = new TestMigrationProfile();

        // Act
        var entityTypes = profile.GetEntityTypes().ToList();

        // Assert
        entityTypes.Should().BeEmpty();
    }

    [Test]
    public void ComplexProfile_TracksAllTasksCorrectly()
    {
        // Arrange
        var profile = new PersonMigrationProfile();

        // Act & Assert
        var personTasks = profile.GetTasksForEntity<Person>().ToList();
        personTasks.Should().HaveCount(4); // 1 CreateTable + 2 CreateIndex + 1 ImportData
        
        var entityTypes = profile.GetEntityTypes().ToList();
        entityTypes.Should().ContainSingle().Which.Should().Be(typeof(Person));
        
        profile.Tasks.Should().HaveCount(4);
        profile.Tasks.Should().OnlyContain(t => t.EntityType == typeof(Person));
    }

    private class TestMigrationProfile : MigrationProfile
    {
        // Empty profile for testing
    }
}