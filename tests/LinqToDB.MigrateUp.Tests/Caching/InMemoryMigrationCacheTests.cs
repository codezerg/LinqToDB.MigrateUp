using FluentAssertions;
using LinqToDB.MigrateUp.Caching;
using LinqToDB.MigrateUp.Tests.TestEntities;
using NUnit.Framework;

namespace LinqToDB.MigrateUp.Tests.Caching;

[TestFixture]
public class InMemoryMigrationCacheTests
{
    private InMemoryMigrationCache _cache = null!;

    [SetUp]
    public void SetUp()
    {
        _cache = new InMemoryMigrationCache();
    }

    [Test]
    public void IsTaskExecuted_ReturnsFalse_WhenTaskNotCached()
    {
        // Arrange
        var entityType = typeof(Person);
        var taskType = typeof(string);
        var taskHash = "test-hash";

        // Act
        var result = _cache.IsTaskExecuted(entityType, taskType, taskHash);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void MarkTaskExecuted_AndIsTaskExecuted_ReturnsTrue()
    {
        // Arrange
        var entityType = typeof(Person);
        var taskType = typeof(string);
        var taskHash = "test-hash";

        // Act
        _cache.MarkTaskExecuted(entityType, taskType, taskHash);
        var result = _cache.IsTaskExecuted(entityType, taskType, taskHash);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsTaskExecuted_ReturnsFalse_ForDifferentHash()
    {
        // Arrange
        var entityType = typeof(Person);
        var taskType = typeof(string);
        var taskHash1 = "test-hash-1";
        var taskHash2 = "test-hash-2";

        // Act
        _cache.MarkTaskExecuted(entityType, taskType, taskHash1);
        var result = _cache.IsTaskExecuted(entityType, taskType, taskHash2);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsTaskExecuted_ReturnsFalse_ForDifferentEntityType()
    {
        // Arrange
        var taskType = typeof(string);
        var taskHash = "test-hash";

        // Act
        _cache.MarkTaskExecuted(typeof(Person), taskType, taskHash);
        var result = _cache.IsTaskExecuted(typeof(Product), taskType, taskHash);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetCachedEntityTypes_ReturnsEmpty_WhenNothingCached()
    {
        // Act
        var result = _cache.GetCachedEntityTypes();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetCachedEntityTypes_ReturnsEntityTypes_WhenTasksCached()
    {
        // Arrange
        var personType = typeof(Person);
        var productType = typeof(Product);
        var taskType = typeof(string);

        // Act
        _cache.MarkTaskExecuted(personType, taskType, "hash1");
        _cache.MarkTaskExecuted(productType, taskType, "hash2");
        var result = _cache.GetCachedEntityTypes().ToList();

        // Assert
        result.Should().Contain(personType);
        result.Should().Contain(productType);
        result.Should().HaveCount(2);
    }

    [Test]
    public void ClearEntityCache_RemovesOnlySpecifiedEntity()
    {
        // Arrange
        var personType = typeof(Person);
        var productType = typeof(Product);
        var taskType = typeof(string);

        _cache.MarkTaskExecuted(personType, taskType, "hash1");
        _cache.MarkTaskExecuted(productType, taskType, "hash2");

        // Act
        _cache.ClearEntityCache(personType);

        // Assert
        _cache.IsTaskExecuted(personType, taskType, "hash1").Should().BeFalse();
        _cache.IsTaskExecuted(productType, taskType, "hash2").Should().BeTrue();
    }

    [Test]
    public void ClearAll_RemovesAllCachedEntries()
    {
        // Arrange
        var personType = typeof(Person);
        var productType = typeof(Product);
        var taskType = typeof(string);

        _cache.MarkTaskExecuted(personType, taskType, "hash1");
        _cache.MarkTaskExecuted(productType, taskType, "hash2");

        // Act
        _cache.ClearAll();

        // Assert
        _cache.IsTaskExecuted(personType, taskType, "hash1").Should().BeFalse();
        _cache.IsTaskExecuted(productType, taskType, "hash2").Should().BeFalse();
        _cache.GetCachedEntityTypes().Should().BeEmpty();
    }

    [Test]
    public void MarkTaskExecuted_HandlesNullInputs_Gracefully()
    {
        // Act & Assert - Should not throw
        _cache.MarkTaskExecuted(null!, typeof(string), "hash");
        _cache.MarkTaskExecuted(typeof(Person), null!, "hash");
        _cache.MarkTaskExecuted(typeof(Person), typeof(string), null!);
        _cache.MarkTaskExecuted(typeof(Person), typeof(string), "");
    }

    [Test]
    public void IsTaskExecuted_HandlesNullInputs_ReturnsFalse()
    {
        // Act & Assert
        _cache.IsTaskExecuted(null!, typeof(string), "hash").Should().BeFalse();
        _cache.IsTaskExecuted(typeof(Person), null!, "hash").Should().BeFalse();
        _cache.IsTaskExecuted(typeof(Person), typeof(string), null!).Should().BeFalse();
        _cache.IsTaskExecuted(typeof(Person), typeof(string), "").Should().BeFalse();
    }

    [Test]
    public void ClearEntityCache_HandlesNullInput_Gracefully()
    {
        // Act & Assert - Should not throw
        _cache.ClearEntityCache(null!);
    }

    [Test]
    public void Cache_IsThreadSafe()
    {
        // Arrange
        var entityType = typeof(Person);
        var taskType = typeof(string);
        var tasks = new List<Task>();

        // Act - Multiple threads marking tasks as executed
        for (int i = 0; i < 10; i++)
        {
            var hash = $"hash-{i}";
            tasks.Add(Task.Run(() => _cache.MarkTaskExecuted(entityType, taskType, hash)));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        for (int i = 0; i < 10; i++)
        {
            _cache.IsTaskExecuted(entityType, taskType, $"hash-{i}").Should().BeTrue();
        }
    }
}