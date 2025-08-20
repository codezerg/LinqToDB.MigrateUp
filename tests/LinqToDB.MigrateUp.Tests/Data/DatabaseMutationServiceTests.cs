using FluentAssertions;
using LinqToDB.MigrateUp.Data;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.MigrateUp.Sql;
using LinqToDB.MigrateUp.Tests.Testing;
using NUnit.Framework;

namespace LinqToDB.MigrateUp.Tests.Data;

[TestFixture]
public class DatabaseMutationServiceTests
{
    private MockDataConnectionService _dataService = null!;
    private SqlServerQueryService _queryService = null!;
    private DatabaseMutationService _mutationService = null!;

    [SetUp]
    public void SetUp()
    {
        _dataService = new MockDataConnectionService();
        _queryService = new SqlServerQueryService();
        _mutationService = new DatabaseMutationService(_dataService, _queryService);
    }

    [Test]
    public void AlterTableColumn_GeneratesCorrectSql_WithoutDuplicateColumnName()
    {
        // Arrange
        var tableName = "Person";
        var columnName = "FirstName";
        var newColumn = new TableColumn(
            columnName: "FirstName",
            dataType: "VarChar(150)",
            isNullable: true
        );

        // Act
        _mutationService.AlterTableColumn(tableName, columnName, newColumn);

        // Assert
        var executedCommands = _dataService.GetExecutedCommands();
        executedCommands.Should().HaveCount(1);
        
        var expectedSql = "ALTER TABLE [Person] ALTER COLUMN [FirstName] VarChar(150) NULL";
        executedCommands[0].Should().Be(expectedSql);
    }

    [Test]
    public void AlterTableColumn_NonNullableColumn_GeneratesCorrectSql()
    {
        // Arrange
        var tableName = "Person";
        var columnName = "LastName";
        var newColumn = new TableColumn(
            columnName: "LastName",
            dataType: "NVarChar(200)",
            isNullable: false
        );

        // Act
        _mutationService.AlterTableColumn(tableName, columnName, newColumn);

        // Assert
        var executedCommands = _dataService.GetExecutedCommands();
        executedCommands.Should().HaveCount(1);
        
        var expectedSql = "ALTER TABLE [Person] ALTER COLUMN [LastName] NVarChar(200) NOT NULL";
        executedCommands[0].Should().Be(expectedSql);
    }

    [Test]
    public void CreateTableColumn_IncludesColumnNameInDefinition()
    {
        // Arrange
        var tableName = "Person";
        var column = new TableColumn(
            columnName: "Email",
            dataType: "VarChar(254)",
            isNullable: true
        );

        // Act
        _mutationService.CreateTableColumn<object>(tableName, column);

        // Assert
        var executedCommands = _dataService.GetExecutedCommands();
        executedCommands.Should().HaveCount(1);
        
        var expectedSql = "ALTER TABLE [Person] ADD [Email] VarChar(254) NULL";
        executedCommands[0].Should().Be(expectedSql);
    }
}