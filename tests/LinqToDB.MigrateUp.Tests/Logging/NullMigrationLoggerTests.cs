using FluentAssertions;
using LinqToDB.MigrateUp.Logging;
using NUnit.Framework;

namespace LinqToDB.MigrateUp.Tests.Logging;

[TestFixture]
public class NullMigrationLoggerTests
{
    private NullMigrationLogger _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = new NullMigrationLogger();
    }

    [Test]
    public void WriteInfo_DoesNotThrow()
    {
        // Act & Assert
        _logger.Invoking(l => l.WriteInfo("Test info message"))
            .Should().NotThrow();
    }

    [Test]
    public void WriteWarning_DoesNotThrow()
    {
        // Act & Assert
        _logger.Invoking(l => l.WriteWarning("Test warning message"))
            .Should().NotThrow();
    }

    [Test]
    public void WriteError_DoesNotThrow()
    {
        // Act & Assert
        _logger.Invoking(l => l.WriteError("Test error message"))
            .Should().NotThrow();
    }

    [Test]
    public void WriteError_WithException_DoesNotThrow()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act & Assert
        _logger.Invoking(l => l.WriteError("Test error message", exception))
            .Should().NotThrow();
    }

    [Test]
    public void WriteError_WithNullException_DoesNotThrow()
    {
        // Act & Assert
        _logger.Invoking(l => l.WriteError("Test error message", null))
            .Should().NotThrow();
    }

    [Test]
    public void AllMethods_HandleNullMessages_Gracefully()
    {
        // Act & Assert
        _logger.Invoking(l => l.WriteInfo(null!)).Should().NotThrow();
        _logger.Invoking(l => l.WriteWarning(null!)).Should().NotThrow();
        _logger.Invoking(l => l.WriteError(null!)).Should().NotThrow();
    }

    [Test]
    public void AllMethods_HandleEmptyMessages_Gracefully()
    {
        // Act & Assert
        _logger.Invoking(l => l.WriteInfo("")).Should().NotThrow();
        _logger.Invoking(l => l.WriteWarning("")).Should().NotThrow();
        _logger.Invoking(l => l.WriteError("")).Should().NotThrow();
    }
}