using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Abstracts database operations to enable testability and separation of concerns.
    /// </summary>
    public interface IDataConnectionService
    {
        /// <summary>
        /// Creates a table for the specified entity type.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        void CreateTable<T>() where T : class;

        /// <summary>
        /// Gets a queryable table for the specified entity type.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <returns>A queryable table.</returns>
        IQueryable<T> GetTable<T>() where T : class;

        /// <summary>
        /// Executes a SQL command and returns the number of affected rows.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <returns>The number of affected rows.</returns>
        int Execute(string sql);

        /// <summary>
        /// Performs bulk copy operation for the specified items.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="items">The items to bulk copy.</param>
        void BulkCopy<T>(IEnumerable<T> items) where T : class;

        /// <summary>
        /// Gets the entity name for the specified type.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <returns>The entity name.</returns>
        string GetEntityName<T>() where T : class;
    }
}