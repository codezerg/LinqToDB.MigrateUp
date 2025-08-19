using FluentAssertions;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Configuration;
using LinqToDB.MigrateUp.Expressions;
using LinqToDB.MigrateUp.Tests.Infrastructure;
using NUnit.Framework;

namespace LinqToDB.MigrateUp.Tests;

[TestFixture]
public class AlterColumnFeatureTests
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
    public void AlterColumn_DirectSQLiteRecreation()
    {
        // Test the SQLite table recreation logic directly
        
        // Create initial table
        _connection.Execute(@"
            CREATE TABLE TestTable (
                Id INTEGER PRIMARY KEY,
                Value INTEGER NULL
            )");
        
        // Insert data
        _connection.Execute("INSERT INTO TestTable (Id, Value) VALUES (1, 100)");
        
        // Verify initial state
        var before = _connection.Query<dynamic>("PRAGMA table_info(TestTable)").ToList();
        dynamic? valueBefore = before.FirstOrDefault(c => c.name == "Value");
        Assert.That((long)valueBefore!.notnull, Is.EqualTo(0L), "Should be nullable initially");
        
        // Recreate table with NOT NULL
        var tempTable = "TestTable_temp";
        _connection.Execute($@"
            CREATE TABLE {tempTable} (
                Id INTEGER PRIMARY KEY,
                Value INTEGER NOT NULL
            )");
        
        _connection.Execute($"INSERT INTO {tempTable} SELECT * FROM TestTable");
        _connection.Execute("DROP TABLE TestTable");
        _connection.Execute($"ALTER TABLE {tempTable} RENAME TO TestTable");
        
        // Verify final state
        var after = _connection.Query<dynamic>("PRAGMA table_info(TestTable)").ToList();
        dynamic? valueAfter = after.FirstOrDefault(c => c.name == "Value");
        Assert.That((long)valueAfter!.notnull, Is.EqualTo(1L), "Should be NOT NULL after recreation");
    }

    [Test]
    public void AlterColumn_TaskGetsExecuted()
    {
        // Arrange - First create table
        var migration = _database.CreateMigration();
        var createConfig = new MigrationConfiguration(cfg =>
        {
            cfg.AddProfile(new CreateTestTableProfile());
        });
        migration.Run(createConfig);

        // Act - Try to alter column
        var alterConfig = new MigrationConfiguration(cfg =>
        {
            var profile = new AlterToNonNullableProfile();
            cfg.AddProfile(profile);
            
            // Check that the task was added
            Assert.That(profile.Tasks.Count, Is.EqualTo(1), "Profile should have one task");
            Assert.That(profile.Tasks[0], Is.InstanceOf<AlterColumnExpression<AlterTestEntity>>(), "Task should be AlterColumnExpression");
        });
        
        // Run the migration
        migration.Run(alterConfig);
        
        // If we get here without exception, the task executed
        Assert.Pass("AlterColumn task executed successfully");
    }

    [Test]
    public void AlterColumn_CanChangeToNonNullable()
    {
        // Arrange - First create table with nullable column
        _connection.Execute(@"
            CREATE TABLE AlterTestTable (
                Id INTEGER PRIMARY KEY,
                Name TEXT,
                OptionalValue INTEGER NULL
            )");
        
        // Insert data with no nulls
        _connection.Execute("INSERT INTO AlterTestTable (Id, Name, OptionalValue) VALUES (1, 'Test1', 100)");
        _connection.Execute("INSERT INTO AlterTestTable (Id, Name, OptionalValue) VALUES (2, 'Test2', 200)");

        // Act - Alter column to non-nullable using the migration
        var migration = _database.CreateMigration();
        var alterConfig = new MigrationConfiguration(cfg =>
        {
            cfg.AddProfile(new AlterToNonNullableProfile());
        });
        migration.Run(alterConfig);

        // Check column state after alter
        var columnsAfterAlter = _connection.Query<dynamic>("PRAGMA table_info(AlterTestTable)").ToList();

        // Assert - Check that column is now non-nullable
        dynamic? valueColumn = columnsAfterAlter.FirstOrDefault(c => c.name == "OptionalValue");
        Assert.That(valueColumn, Is.Not.Null, "Column should exist");
        Assert.That((long)valueColumn.notnull, Is.EqualTo(1L), "Column should be non-nullable");
    }


    [Test]
    public void AlterColumn_CanChangeDataType()
    {
        // Arrange - First create table
        var migration = _database.CreateMigration();
        var createConfig = new MigrationConfiguration(cfg =>
        {
            cfg.AddProfile(new CreateTestTableProfile());
        });
        migration.Run(createConfig);

        // Act - Alter column type
        var alterConfig = new MigrationConfiguration(cfg =>
        {
            cfg.AddProfile(new AlterDataTypeProfile());
        });
        migration.Run(alterConfig);

        // Assert - Check that column type changed
        var columns = _connection.Query<dynamic>("PRAGMA table_info(AlterTestTable)").ToList();
        dynamic? nameColumn = columns.FirstOrDefault(c => c.name == "Name");
        Assert.That(nameColumn, Is.Not.Null, "Column should exist");
        string columnType = nameColumn.type;
        Assert.That(columnType.Contains("VARCHAR(200)"), Is.True, $"Expected VARCHAR(200) but got {columnType}");
    }


    // Test profiles
    private class CreateTestTableProfile : MigrationProfile
    {
        public CreateTestTableProfile()
        {
            this.CreateTable<AlterTestEntity>();
        }
    }

    private class AlterToNonNullableProfile : MigrationProfile
    {
        public AlterToNonNullableProfile()
        {
            this.AlterColumn<AlterTestEntity>()
                .Column(e => e.OptionalValue)
                .NotNullable();
        }
    }

    private class AlterDataTypeProfile : MigrationProfile
    {
        public AlterDataTypeProfile()
        {
            this.AlterColumn<AlterTestEntity>()
                .Column(e => e.Name)
                .ToType("VARCHAR(200)");
        }
    }
}

[Table("AlterTestTable")]
public class AlterTestEntity
{
    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column(Length = 100)]
    public string Name { get; set; } = string.Empty;

    [Column(CanBeNull = true)]
    public int? OptionalValue { get; set; }
}