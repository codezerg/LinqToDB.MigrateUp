using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.MigrateUp.Data;
using LinqToDB.MigrateUp.Execution;
using LinqToDB.SqlProvider;
using LinqToDB.SqlQuery;
using Microsoft.Extensions.Logging;

namespace LinqToDB.MigrateUp.Providers;

/// <summary>
/// Provides a base implementation for migration providers with dependency injection support.
/// </summary>
public class MigrationProviderBase : IMigrationProvider
{
    /// <inheritdoc/>
    public Migration Migration { get; }

    /// <summary>
    /// Gets the database schema service for querying schema information.
    /// </summary>
    protected IDatabaseSchemaService SchemaService { get; }

    /// <summary>
    /// Gets the database mutation service for performing schema changes.
    /// </summary>
    protected IDatabaseMutationService MutationService { get; }

    /// <summary>
    /// Gets the migration state manager for tracking operations.
    /// </summary>
    protected IMigrationStateManager StateManager { get; }

    /// <summary>
    /// Gets the mapping schema associated with the data connection.
    /// </summary>
    protected MappingSchema MappingSchema => Migration.MappingSchema;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationProviderBase"/> class with dependency injection support.
    /// </summary>
    /// <param name="migration">The migration associated with this provider.</param>
    /// <param name="schemaService">The database schema service.</param>
    /// <param name="mutationService">The database mutation service.</param>
    /// <param name="stateManager">The migration state manager.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    protected MigrationProviderBase(
        Migration migration,
        IDatabaseSchemaService schemaService,
        IDatabaseMutationService mutationService,
        IMigrationStateManager stateManager)
    {
        Migration = migration ?? throw new ArgumentNullException(nameof(migration));
        SchemaService = schemaService ?? throw new ArgumentNullException(nameof(schemaService));
        MutationService = mutationService ?? throw new ArgumentNullException(nameof(mutationService));
        StateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
    }



    /// <summary>
    /// Gets the columns for a given entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>A collection of table columns.</returns>
    public virtual IEnumerable<TableColumn> GetEntityColumns<TEntity>() where TEntity : class
    {
        var descriptor = MappingSchema.GetEntityDescriptor(typeof(TEntity));
        return descriptor.Columns
            .Select(x => new TableColumn(x.ColumnName, GetEntityColumnDbType<TEntity>(x), x.CanBeNull))
            .ToList();
    }

    private string GetEntityColumnDbType<TEntity>(ColumnDescriptor column) where TEntity : class
    {
        // This is a simplified implementation - in a full service abstraction,
        // this would be handled by a specialized service that can build SQL types
        // without direct database provider access
        try
        {
            var dataContext = Migration.DataService.GetDataContext();
            if (dataContext is DataConnection dataConnection)
            {
                var dataProvider = dataConnection.DataProvider;
                var sqlBuilder = dataProvider.CreateSqlBuilder(MappingSchema, dataConnection.Options);
                if (sqlBuilder != null)
                {
                    var sqlDataType = new SqlDataType(column.GetDbDataType(true));
                    return sqlBuilder.BuildDataType(new System.Text.StringBuilder(), sqlDataType).ToString();
                }
            }
        }
        catch
        {
            // Fall through to default behavior
        }

        // Fallback to basic type mapping if service abstraction doesn't support full provider access
        return column.DataType.ToString();
    }

    /// <inheritdoc/>
    public void UpdateTableSchema<TEntity>() where TEntity : class
    {
        var tableName = Migration.GetEntityName<TEntity>();
        var tableExists = SchemaService.TableExists(tableName);

        if (!tableExists)
        {
            MutationService.CreateTable<TEntity>();
            StateManager.MarkTableCreated(tableName);
            // Table was just created with all columns, no need to add columns
            return;
        }

        var entityColumns = GetEntityColumns<TEntity>().ToList();
        var tableColumns = SchemaService.GetTableColumns(tableName).ToList();

        foreach (var entityColumn in entityColumns)
        {
            var tableColumn = tableColumns.FirstOrDefault(x => string.Equals(x.ColumnName, entityColumn.ColumnName, StringComparison.OrdinalIgnoreCase));
            if (tableColumn == null)
            {
                MutationService.CreateTableColumn<TEntity>(tableName, entityColumn);
            }
            else if (!string.Equals(tableColumn.DataType, entityColumn.DataType, StringComparison.OrdinalIgnoreCase) || tableColumn.IsNullable != entityColumn.IsNullable)
            {
                MutationService.AlterTableColumn(tableName, entityColumn.ColumnName, entityColumn);
            }
        }
    }

    /// <inheritdoc/>
    public void EnsureIndex<TEntity>(string indexName, IEnumerable<TableIndexColumn> columns) where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(indexName))
        {
            throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));
        }

        if (columns == null || !columns.Any())
        {
            throw new ArgumentException("There must be at least one column for the index.", nameof(columns));
        }

        var tableName = Migration.GetEntityName<TEntity>();
        var indexExists = SchemaService.IndexExists(tableName, indexName);

        if (indexExists)
        {
            var currentIndexColumns = SchemaService.GetIndexColumns(tableName, indexName);
            var areColumnsEqual = currentIndexColumns.Select(c => c.ColumnName).SequenceEqual(columns.Select(c => c.ColumnName));

            if (areColumnsEqual)
            {
                return; // Index already exists and hasn't changed
            }

            MutationService.DropTableIndex(tableName, indexName);
        }

        MutationService.CreateTableIndex(tableName, indexName, columns);
        StateManager.MarkIndexCreated(indexName);
    }

    /// <inheritdoc/>
    public virtual void AlterColumn<TEntity>(string tableName, string columnName, TableColumn newColumn) where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));
        
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Column name cannot be null or whitespace.", nameof(columnName));
        
        if (newColumn == null)
            throw new ArgumentNullException(nameof(newColumn));

        // For SQLite, we need special handling as it doesn't support most ALTER COLUMN operations
        var dataContext = Migration.DataService.GetDataContext();
        
        // Check if it's a DataConnection and get the actual provider
        bool isSQLite = false;
        if (dataContext is DataConnection dataConnection)
        {
            var providerName = dataConnection.DataProvider?.Name;
            isSQLite = providerName?.Contains("SQLite", StringComparison.OrdinalIgnoreCase) == true;
        }
        else
        {
            // Fallback to type name checking
            var typeName = dataContext?.GetType().Name;
            var fullName = dataContext?.GetType().FullName;
            isSQLite = typeName?.Contains("SQLite", StringComparison.OrdinalIgnoreCase) == true || 
                       fullName?.Contains("SQLite", StringComparison.OrdinalIgnoreCase) == true;
        }
        
        if (isSQLite)
        {
            AlterColumnForSQLite<TEntity>(tableName, columnName, newColumn);
        }
        else
        {
            // Standard ALTER COLUMN for other databases
            MutationService.AlterTableColumn(tableName, columnName, newColumn);
        }
    }

    /// <inheritdoc/>
    public virtual void RenameColumn<TEntity>(string tableName, string oldColumnName, string newColumnName) where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));
        
        if (string.IsNullOrWhiteSpace(oldColumnName))
            throw new ArgumentException("Old column name cannot be null or whitespace.", nameof(oldColumnName));
        
        if (string.IsNullOrWhiteSpace(newColumnName))
            throw new ArgumentException("New column name cannot be null or whitespace.", nameof(newColumnName));

        // SQLite supports RENAME COLUMN since version 3.25.0 (2018)
        var sql = $"ALTER TABLE {tableName} RENAME COLUMN {oldColumnName} TO {newColumnName}";
        Migration.DataService.Execute(sql);
    }

    /// <summary>
    /// Handles column alteration for SQLite by recreating the table.
    /// </summary>
    private void AlterColumnForSQLite<TEntity>(string tableName, string columnName, TableColumn newColumn) where TEntity : class
    {
        var tempTableName = $"{tableName}_temp_{Guid.NewGuid():N}";
        
        // Get current columns and replace the one being altered
        var columns = SchemaService.GetTableColumns(tableName).ToList();
        var columnIndex = columns.FindIndex(c => 
            string.Equals(c.ColumnName, columnName, StringComparison.OrdinalIgnoreCase));
        
        if (columnIndex < 0)
            throw new InvalidOperationException($"Column {columnName} not found in table {tableName}");
        
        // Important: Use the new column definition, but preserve the correct column name
        // The newColumn might have the wrong name if it was created with just the property name
        var updatedColumn = new TableColumn(
            columnName: columns[columnIndex].ColumnName,  // Keep original column name from DB
            dataType: newColumn.DataType,
            isNullable: newColumn.IsNullable
        );
        columns[columnIndex] = updatedColumn;
        
        // Build CREATE TABLE statement for temp table
        var columnDefinitions = columns.Select(c => 
            $"{c.ColumnName} {c.DataType} {(c.IsNullable ? "NULL" : "NOT NULL")}");
        
        var createTableSql = $"CREATE TABLE {tempTableName} ({string.Join(", ", columnDefinitions)})";
        
        // Execute table recreation in a transaction-like manner
        try
        {
            // 1. Create temporary table with new schema
            Migration.DataService.Execute(createTableSql);
            
            // 2. Copy data from original table
            var columnNames = string.Join(", ", columns.Select(c => c.ColumnName));
            var copySql = $"INSERT INTO {tempTableName} ({columnNames}) SELECT {columnNames} FROM {tableName}";
            Migration.DataService.Execute(copySql);
            
            // 3. Drop original table
            Migration.DataService.Execute($"DROP TABLE {tableName}");
            
            // 4. Rename temporary table to original name
            Migration.DataService.Execute($"ALTER TABLE {tempTableName} RENAME TO {tableName}");
            
        }
        catch (Exception ex)
        {
            Migration.Logger?.LogError(ex, "AlterColumn SQLite failed for column {Column} in table {Table}", columnName, tableName);
            
            // Try to clean up temp table if it exists
            try
            {
                Migration.DataService.Execute($"DROP TABLE IF EXISTS {tempTableName}");
            }
            catch { }
            
            throw;
        }
    }

    /// <inheritdoc/>
    IDatabaseSchemaService IMigrationProvider.SchemaService => SchemaService;
}