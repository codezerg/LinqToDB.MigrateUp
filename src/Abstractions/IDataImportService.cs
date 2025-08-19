using System;
using System.Collections.Generic;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Configuration for data import operations.
    /// </summary>
    public class DataImportConfiguration
    {
        /// <summary>
        /// Gets or sets whether to import data always (default behavior).
        /// </summary>
        public bool ImportAlways { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to import data only when the table is empty.
        /// </summary>
        public bool WhenTableEmpty { get; set; }

        /// <summary>
        /// Gets or sets whether to import data only when the table was created during migration.
        /// </summary>
        public bool WhenTableCreated { get; set; }
    }

    /// <summary>
    /// Provides services for data import operations during migrations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IDataImportService<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Determines whether data import should proceed based on the configuration and current state.
        /// </summary>
        /// <param name="config">The data import configuration.</param>
        /// <param name="stateManager">The migration state manager.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="tableHasData">Whether the table contains any data.</param>
        /// <returns>True if import should proceed, false otherwise.</returns>
        bool ShouldImport(DataImportConfiguration config, IMigrationStateManager stateManager, string tableName, bool tableHasData);

        /// <summary>
        /// Filters source items to only include those that should be inserted (not already existing).
        /// </summary>
        /// <param name="sourceItems">The source items to import.</param>
        /// <param name="existingItems">The items that already exist in the database.</param>
        /// <param name="matchFunctions">Functions to determine if a source item matches an existing item.</param>
        /// <returns>Items that should be inserted.</returns>
        IEnumerable<TEntity> GetItemsToInsert(
            IEnumerable<TEntity> sourceItems, 
            IEnumerable<TEntity> existingItems,
            IEnumerable<Func<TEntity, bool>> matchFunctions);
    }
}