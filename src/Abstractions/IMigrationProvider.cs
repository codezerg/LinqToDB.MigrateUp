using LinqToDB.Data;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.MigrateUp.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp.Abstractions
{
    /// <summary>
    /// Defines the contract for migration providers.
    /// </summary>
    public interface IMigrationProvider
    {
        /// <summary>
        /// Gets the migration associated with the provider.
        /// </summary>
        Migration Migration { get; }


        /// <summary>
        /// Applies schema updates to the database table associated with the specified entity type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type representing the database table.</typeparam>
        /// <param name="options">The options for schema update operation.</param>
        /// <returns>A boolean indicating whether changes were made to the schema.</returns>
        void UpdateTableSchema<TEntity>() where TEntity : class;


        /// <summary>
        /// Ensures the existence and correct structure of an index in the database.
        /// </summary>
        /// <typeparam name="TEntity">The entity type representing the database table.</typeparam>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="indexDefinition">The definition of the index structure.</param>
        /// <returns>A boolean indicating whether changes were made to the index.</returns>
        void EnsureIndex<TEntity>(string indexName, IEnumerable<TableIndexColumn> columns) where TEntity : class;
    }
}
