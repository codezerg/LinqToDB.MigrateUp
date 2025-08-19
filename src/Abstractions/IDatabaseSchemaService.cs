using LinqToDB.MigrateUp.Schema;
using LinqToDB.MigrateUp.Abstractions;
using System.Collections.Generic;

namespace LinqToDB.MigrateUp.Abstractions
{
    /// <summary>
    /// Provides database schema query operations abstracted from specific database implementations.
    /// </summary>
    public interface IDatabaseSchemaService
    {
        /// <summary>
        /// Checks if a table exists in the database.
        /// </summary>
        /// <param name="tableName">The name of the table to check.</param>
        /// <returns>True if the table exists, false otherwise.</returns>
        bool TableExists(string tableName);

        /// <summary>
        /// Checks if an index exists on the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>True if the index exists, false otherwise.</returns>
        bool IndexExists(string tableName, string indexName);

        /// <summary>
        /// Gets the column definitions for the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A collection of table columns.</returns>
        IEnumerable<TableColumn> GetTableColumns(string tableName);

        /// <summary>
        /// Gets the column definitions for the specified index.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>A collection of table index columns.</returns>
        IEnumerable<TableIndexColumn> GetIndexColumns(string tableName, string indexName);
    }
}