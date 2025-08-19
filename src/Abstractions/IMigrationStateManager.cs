using System;
using LinqToDB.MigrateUp.Abstractions;

namespace LinqToDB.MigrateUp.Abstractions
{
    /// <summary>
    /// Manages the state of migration operations, tracking what has been created or modified.
    /// </summary>
    public interface IMigrationStateManager
    {
        /// <summary>
        /// Marks a table as created during the migration.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        void MarkTableCreated(string tableName);

        /// <summary>
        /// Marks an index as created during the migration.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        void MarkIndexCreated(string indexName);

        /// <summary>
        /// Checks if a table was created during the migration.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>True if the table was created, false otherwise.</returns>
        bool IsTableCreated(string tableName);

        /// <summary>
        /// Checks if an index was created during the migration.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>True if the index was created, false otherwise.</returns>
        bool IsIndexCreated(string indexName);

        /// <summary>
        /// Resets the migration state, clearing all tracked operations.
        /// </summary>
        void Reset();

        /// <summary>
        /// Event raised when a table is created.
        /// </summary>
        event EventHandler<string>? TableCreated;

        /// <summary>
        /// Event raised when an index is created.
        /// </summary>
        event EventHandler<string>? IndexCreated;
    }
}