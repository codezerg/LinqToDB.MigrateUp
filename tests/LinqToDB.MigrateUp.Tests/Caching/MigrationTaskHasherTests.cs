using FluentAssertions;
using LinqToDB.MigrateUp.Caching;
using LinqToDB.MigrateUp.Tests.TestProfiles;
using NUnit.Framework;

namespace LinqToDB.MigrateUp.Tests.Caching;

[TestFixture]
public class MigrationTaskHasherTests
{
    [Test]
    public void GenerateHash_ReturnsEmptyString_ForNullTask()
    {
        // Act
        var hash = MigrationTaskHasher.GenerateHash(null!);

        // Assert
        hash.Should().BeEmpty();
    }

    [Test]
    public void GenerateHash_ReturnsSameHash_ForSameTaskType()
    {
        // Arrange
        var profile1 = new PersonMigrationProfile();
        var profile2 = new PersonMigrationProfile();
        var task1 = profile1.Tasks.First();
        var task2 = profile2.Tasks.First();

        // Act
        var hash1 = MigrationTaskHasher.GenerateHash(task1);
        var hash2 = MigrationTaskHasher.GenerateHash(task2);

        // Assert
        hash1.Should().Be(hash2);
        hash1.Should().NotBeEmpty();
    }

    [Test]
    public void GenerateHash_ReturnsDifferentHashes_ForDifferentTaskTypes()
    {
        // Arrange
        var personProfile = new PersonMigrationProfile();
        var productProfile = new ProductMigrationProfile();
        var personTask = personProfile.Tasks.First();
        var productTask = productProfile.Tasks.First();

        // Act
        var personHash = MigrationTaskHasher.GenerateHash(personTask);
        var productHash = MigrationTaskHasher.GenerateHash(productTask);

        // Assert
        personHash.Should().NotBe(productHash);
        personHash.Should().NotBeEmpty();
        productHash.Should().NotBeEmpty();
    }

    [Test]
    public void GenerateHash_ReturnsValidSHA256Hash()
    {
        // Arrange
        var profile = new PersonMigrationProfile();
        var task = profile.Tasks.First();

        // Act
        var hash = MigrationTaskHasher.GenerateHash(task);

        // Assert
        hash.Should().NotBeEmpty();
        hash.Should().HaveLength(64); // SHA256 produces 64 character hex string
        hash.Should().MatchRegex("^[a-f0-9]+$"); // Should be lowercase hexadecimal
    }

    [Test]
    public void GenerateHash_IsConsistent_AcrossMultipleCalls()
    {
        // Arrange
        var profile = new PersonMigrationProfile();
        var task = profile.Tasks.First();

        // Act
        var hash1 = MigrationTaskHasher.GenerateHash(task);
        var hash2 = MigrationTaskHasher.GenerateHash(task);
        var hash3 = MigrationTaskHasher.GenerateHash(task);

        // Assert
        hash1.Should().Be(hash2);
        hash2.Should().Be(hash3);
    }
}