using FluentAssertions;
using LinqToDB.MigrateUp.Logging;
using NUnit.Framework;
using System.Text;

namespace LinqToDB.MigrateUp.Tests.Logging;

[TestFixture]
public class ConsoleMigrationLoggerTests
{
    private ConsoleMigrationLogger _logger = null!;
    private StringWriter _consoleOutput = null!;
    private StringWriter _consoleError = null!;
    private TextWriter _originalOut = null!;
    private TextWriter _originalError = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = new ConsoleMigrationLogger();
        
        // Capture console output
        _originalOut = Console.Out;
        _originalError = Console.Error;
        _consoleOutput = new StringWriter();
        _consoleError = new StringWriter();
        Console.SetOut(_consoleOutput);
        Console.SetError(_consoleError);
    }

    [TearDown]
    public void TearDown()
    {
        // Restore console output
        Console.SetOut(_originalOut);
        Console.SetError(_originalError);
        _consoleOutput?.Dispose();
        _consoleError?.Dispose();
    }

    [Test]
    public void WriteInfo_WritesToConsoleOutput()
    {
        // Act
        _logger.WriteInfo("Test info message");

        // Assert
        var output = _consoleOutput.ToString();
        output.Should().Contain("INFO: Test info message");
        output.Should().Contain(DateTime.Now.ToString("yyyy-MM-dd"));
    }

    [Test]
    public void WriteWarning_WritesToConsoleOutput()
    {
        // Act
        _logger.WriteWarning("Test warning message");

        // Assert
        var output = _consoleOutput.ToString();
        output.Should().Contain("WARNING: Test warning message");
        output.Should().Contain(DateTime.Now.ToString("yyyy-MM-dd"));
    }

    [Test]
    public void WriteError_WritesToConsoleError()
    {
        // Act
        _logger.WriteError("Test error message");

        // Assert
        var errorOutput = _consoleError.ToString();
        errorOutput.Should().Contain("ERROR: Test error message");
        errorOutput.Should().Contain(DateTime.Now.ToString("yyyy-MM-dd"));
    }

    [Test]
    public void WriteError_WithException_WritesExceptionDetails()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        _logger.WriteError("Test error message", exception);

        // Assert
        var errorOutput = _consoleError.ToString();
        errorOutput.Should().Contain("ERROR: Test error message");
        errorOutput.Should().Contain("ERROR: System.InvalidOperationException: Test exception");
    }

    [Test]
    public void WriteError_WithNullException_DoesNotThrow()
    {
        // Act & Assert
        _logger.Invoking(l => l.WriteError("Test error message", null))
            .Should().NotThrow();

        var errorOutput = _consoleError.ToString();
        errorOutput.Should().Contain("ERROR: Test error message");
        errorOutput.Should().NotContain("System.InvalidOperationException");
    }

    [Test]
    public void AllLogMethods_IncludeTimestamp()
    {
        // Arrange
        var now = DateTime.Now;

        // Act
        _logger.WriteInfo("Info");
        _logger.WriteWarning("Warning");
        _logger.WriteError("Error");

        // Assert
        var standardOutput = _consoleOutput.ToString();
        var errorOutput = _consoleError.ToString();
        var combinedOutput = standardOutput + errorOutput;

        combinedOutput.Should().Contain(now.ToString("yyyy-MM-dd"));
        combinedOutput.Should().Contain(now.ToString("HH:mm")); // At least hour and minute should match
    }

    [Test]
    public void LogMessages_AreFormatted_Consistently()
    {
        // Act
        _logger.WriteInfo("Test message");

        // Assert
        var output = _consoleOutput.ToString();
        output.Should().MatchRegex(@"\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\] INFO: Test message");
    }
}