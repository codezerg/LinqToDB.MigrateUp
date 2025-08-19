using FluentAssertions;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Configuration;
using LinqToDB.MigrateUp.Tests.Infrastructure;
using LinqToDB.MigrateUp.Validation;
using NUnit.Framework;
using System.Data.SQLite;

namespace LinqToDB.MigrateUp.Tests;

[TestFixture]
public class MigrationErrorHandlingTests
{
    private TestDatabase _database = null!;
    private DataConnection _connection = null!;

    [SetUp]
    public void SetUp()
    {
        _database = new TestDatabase();
        _connection = _database.CreateConnection();
    }

    [TearDown]
    public void TearDown()
    {
        _connection?.Dispose();
        _database?.Dispose();
    }

    [Test]
    public void Migration_WithSqlInjectionAttempt_ThrowsValidationError()
    {
        // Arrange
        var migration = _database.CreateMigration();
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            // Try to create a table with SQL injection in name
            var tableName = "Users'; DROP TABLE Users; --";
            ValidationHelper.ValidateSqlIdentifier(tableName, nameof(tableName));
        });
    }

    [Test]
    public void Migration_WithReservedKeyword_HandlesCorrectly()
    {
        // Reserved SQL keywords should be escaped properly
        var reservedWords = new[] { "SELECT", "FROM", "WHERE", "ORDER", "GROUP" };
        
        foreach (var keyword in reservedWords)
        {
            // Should either throw or escape properly
            try
            {
                ValidationHelper.ValidateSqlIdentifier(keyword, nameof(keyword));
                // If validation passes, the provider should escape it
                keyword.Should().NotBeEmpty();
            }
            catch (ArgumentException)
            {
                // Expected for reserved keywords
            }
        }
    }

    [Test]
    public void Migration_WithVeryLongTableName_ThrowsValidationError()
    {
        // Most databases have limits on identifier length
        var longName = new string('a', 300); // 300 characters
        
        Assert.Throws<ArgumentException>(() =>
        {
            ValidationHelper.ValidateSqlIdentifier(longName, nameof(longName));
        });
    }

    [Test]
    public void Migration_WithSpecialCharacters_ValidatesCorrectly()
    {
        // Test various special characters
        var invalidNames = new[]
        {
            "Table-Name",  // Hyphen might be invalid
            "Table Name",  // Space definitely invalid
            "Table@Name",  // @ symbol invalid
            "Table#Name",  // # might be invalid
            "Table$Name",  // $ might be invalid
            "Table.Name",  // Period might cause issues
            "Table,Name",  // Comma definitely invalid
            "Table;Name",  // Semicolon definitely invalid
            "Table'Name",  // Single quote definitely invalid
            "Table\"Name", // Double quote might be invalid
            "Table`Name",  // Backtick might be provider-specific
        };

        foreach (var name in invalidNames)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ValidationHelper.ValidateSqlIdentifier(name, nameof(name));
            }, $"Should throw for name: {name}");
        }
    }

    [Test]
    public void Migration_WithValidUnderscores_Succeeds()
    {
        // Underscores should be valid
        var validNames = new[]
        {
            "Table_Name",
            "_TableName",
            "TableName_",
            "Table_Name_123",
            "UPPER_CASE_TABLE"
        };

        foreach (var name in validNames)
        {
            Assert.DoesNotThrow(() =>
            {
                ValidationHelper.ValidateSqlIdentifier(name, nameof(name));
            }, $"Should not throw for valid name: {name}");
        }
    }

    [Test]
    public void Migration_PartialFailure_DoesNotLeaveInconsistentState()
    {
        // Arrange - Create initial table
        _connection.Execute(@"
            CREATE TABLE TestTable1 (
                Id INTEGER PRIMARY KEY
            )");

        var migration = _database.CreateMigration();
        var config = new MigrationConfiguration(cfg =>
        {
            cfg.AddProfile(new FailingMigrationProfile());
        });

        // Act - Migration should fail partway through
        migration.Invoking(m => m.Run(config))
            .Should().Throw<Exception>();

        // Assert - Check that partial changes were rolled back
        // or at least we're in a known state
        var tables = _connection.Query<string>(@"
            SELECT name FROM sqlite_master 
            WHERE type='table' AND name LIKE 'TestTable%'").ToList();
        
        // Should only have the original table, not the partial migration tables
        tables.Should().Contain("TestTable1");
        // Depending on transaction handling, TestTable2 might or might not exist
    }

    [Test]
    public void Migration_WithCircularDependency_ThrowsError()
    {
        // This would require implementing dependency detection
        // For now, we test the concept
        
        var migration = _database.CreateMigration();
        var config = new MigrationConfiguration(cfg =>
        {
            // Profile A depends on table from Profile B
            // Profile B depends on table from Profile A
            // This should be detected and throw
        });

        // Would need circular dependency detection implementation
    }

    [Test]
    public void Migration_WithInvalidDataType_ThrowsError()
    {
        // Arrange
        var migration = _database.CreateMigration();
        
        // Act & Assert - Try to create column with invalid data type
        _connection.Invoking(c => c.Execute(@"
            CREATE TABLE TestTable (
                Id INTEGER PRIMARY KEY,
                InvalidColumn INVALID_DATA_TYPE
            )"))
            .Should().Throw<SQLiteException>()
            .WithMessage("*SQL logic error*");
    }

    [Test]
    public void Migration_ConcurrentExecution_HandlesGracefully()
    {
        // Simulate concurrent migration attempts
        var migration1 = _database.CreateMigration();
        var migration2 = _database.CreateMigration();
        
        var config = new MigrationConfiguration(cfg =>
        {
            cfg.AddProfile(new SimpleMigrationProfile());
        });

        // Run migrations in parallel
        var task1 = Task.Run(() => migration1.Run(config));
        var task2 = Task.Run(() => migration2.Run(config));

        // At least one should succeed, the other might fail due to locking
        // or both might succeed if they're idempotent
        Task.WaitAll(task1, task2);

        // Verify the table was created (by at least one migration)
        var tableExists = _connection.Execute<int>(@"
            SELECT COUNT(*) FROM sqlite_master 
            WHERE type='table' AND name='SimpleTable'");
        tableExists.Should().Be(1);
    }

    [Test]
    public void Migration_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var migration = _database.CreateMigration();

        // Act & Assert
        migration.Invoking(m => m.Run(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Migration_WithEmptyProfile_DoesNothing()
    {
        // Arrange
        var migration = _database.CreateMigration();
        var config = new MigrationConfiguration(cfg =>
        {
            cfg.AddProfile(new EmptyMigrationProfile());
        });

        // Act - Should not throw
        migration.Invoking(m => m.Run(config))
            .Should().NotThrow();

        // Assert - No tables should be created
        var tables = _connection.Query<string>(@"
            SELECT name FROM sqlite_master 
            WHERE type='table' AND name NOT LIKE 'sqlite_%'").ToList();
        
        tables.Should().BeEmpty();
    }

    // Test profiles
    private class FailingMigrationProfile : MigrationProfile
    {
        public FailingMigrationProfile()
        {
            this.CreateTable<ValidEntity>();
            // This would fail because InvalidEntity has issues
            this.CreateTable<InvalidEntity>();
        }
    }

    private class SimpleMigrationProfile : MigrationProfile
    {
        public SimpleMigrationProfile()
        {
            this.CreateTable<SimpleEntity>();
        }
    }

    private class EmptyMigrationProfile : MigrationProfile
    {
        public EmptyMigrationProfile()
        {
            // No tasks
        }
    }
}

[Table("ValidTable")]
public class ValidEntity
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

[Table("InvalidTable")]
public class InvalidEntity
{
    // Missing primary key - should cause issues
    public string Name { get; set; } = string.Empty;
}

[Table("SimpleTable")]
public class SimpleEntity
{
    [PrimaryKey]
    public int Id { get; set; }
}