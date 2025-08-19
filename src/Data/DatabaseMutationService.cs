using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.MigrateUp.Sql;
using LinqToDB.MigrateUp.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToDB.MigrateUp.Data;

/// <summary>
/// Default implementation of IDatabaseMutationService using query services and data connection service.
/// </summary>
public class DatabaseMutationService : IDatabaseMutationService
{
    private readonly IDataConnectionService _dataService;
    private readonly ISqlQueryService _queryService;

    /// <summary>
    /// Initializes a new instance of the DatabaseMutationService class.
    /// </summary>
    /// <param name="dataService">The data connection service.</param>
    /// <param name="queryService">The SQL query service.</param>
    public DatabaseMutationService(IDataConnectionService dataService, ISqlQueryService queryService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
    }

    /// <inheritdoc/>
    public void CreateTable<TEntity>() where TEntity : class
    {
        _dataService.CreateTable<TEntity>();
    }

    /// <inheritdoc/>
    public void CreateTableColumn<TTable>(string tableName, TableColumn column)
    {
        ValidationHelper.ValidateSqlIdentifier(tableName, nameof(tableName));
        
        if (column == null)
            throw new ArgumentNullException(nameof(column));

        ValidationHelper.ValidateSqlIdentifier(column.ColumnName, nameof(column.ColumnName));

        var columnDefinition = BuildColumnDefinition(column);
        var command = _queryService.BuildAddColumnCommand(tableName, columnDefinition);
        _dataService.Execute(command);
    }

    /// <inheritdoc/>
    public void AlterTableColumn(string tableName, string columnName, TableColumn newColumn)
    {
        ValidationHelper.ValidateSqlIdentifier(tableName, nameof(tableName));
        ValidationHelper.ValidateSqlIdentifier(columnName, nameof(columnName));
        
        if (newColumn == null)
            throw new ArgumentNullException(nameof(newColumn));
            
        ValidationHelper.ValidateSqlIdentifier(newColumn.ColumnName, nameof(newColumn.ColumnName));

        var columnDefinition = BuildColumnDefinition(newColumn);
        var command = _queryService.BuildAlterColumnCommand(tableName, columnName, columnDefinition);
        _dataService.Execute(command);
    }

    /// <inheritdoc/>
    public void CreateTableIndex(string tableName, string indexName, IEnumerable<TableIndexColumn> columns)
    {
        ValidationHelper.ValidateSqlIdentifier(tableName, nameof(tableName));
        ValidationHelper.ValidateSqlIdentifier(indexName, nameof(indexName));
        
        if (columns == null || !columns.Any())
            throw new ArgumentException("At least one column is required for the index.", nameof(columns));

        // Validate each column name
        foreach (var column in columns)
        {
            ValidationHelper.ValidateSqlIdentifier(column.ColumnName, nameof(column.ColumnName));
        }

        var columnNames = columns.Select(c => c.ColumnName).ToArray();
        var command = _queryService.BuildCreateIndexCommand(tableName, indexName, columnNames);
        _dataService.Execute(command);
    }

    /// <inheritdoc/>
    public void DropTableIndex(string tableName, string indexName)
    {
        ValidationHelper.ValidateSqlIdentifier(tableName, nameof(tableName));
        ValidationHelper.ValidateSqlIdentifier(indexName, nameof(indexName));

        var command = _queryService.BuildDropIndexCommand(tableName, indexName);
        _dataService.Execute(command);
    }

    private string BuildColumnDefinition(TableColumn column)
    {
        var nullable = column.IsNullable ? "NULL" : "NOT NULL";
        return $"[{column.ColumnName}] {column.DataType} {nullable}";
    }
}