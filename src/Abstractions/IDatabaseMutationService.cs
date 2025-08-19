using LinqToDB.MigrateUp.Schema;
using LinqToDB.MigrateUp.Abstractions;
using System.Collections.Generic;

namespace LinqToDB.MigrateUp.Abstractions;

/// <summary>
/// Provides database mutation operations abstracted from specific database implementations.
/// </summary>
public interface IDatabaseMutationService
{
    /// <summary>
    /// Creates a new table based on the entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity representing the table.</typeparam>
    void CreateTable<TEntity>() where TEntity : class;

    /// <summary>
    /// Creates a new column in the specified table.
    /// </summary>
    /// <typeparam name="TTable">The type of the table entity.</typeparam>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="column">The column definition to create.</param>
    void CreateTableColumn<TTable>(string tableName, TableColumn column);

    /// <summary>
    /// Alters an existing column in the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column to alter.</param>
    /// <param name="newColumn">The new column definition.</param>
    void AlterTableColumn(string tableName, string columnName, TableColumn newColumn);

    /// <summary>
    /// Creates a new index on the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="indexName">The name of the index to create.</param>
    /// <param name="columns">The columns to include in the index.</param>
    void CreateTableIndex(string tableName, string indexName, IEnumerable<TableIndexColumn> columns);

    /// <summary>
    /// Drops an existing index from the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="indexName">The name of the index to drop.</param>
    void DropTableIndex(string tableName, string indexName);
}