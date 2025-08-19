using LinqToDB.Data;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.MigrateUp.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp.Abstractions;

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

    /// <summary>
    /// Alters a column in the database table.
    /// </summary>
    /// <typeparam name="TEntity">The entity type representing the database table.</typeparam>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column to alter.</param>
    /// <param name="newColumn">The new column definition.</param>
    void AlterColumn<TEntity>(string tableName, string columnName, TableColumn newColumn) where TEntity : class;

    /// <summary>
    /// Renames a column in the database table.
    /// </summary>
    /// <typeparam name="TEntity">The entity type representing the database table.</typeparam>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="oldColumnName">The current name of the column.</param>
    /// <param name="newColumnName">The new name for the column.</param>
    void RenameColumn<TEntity>(string tableName, string oldColumnName, string newColumnName) where TEntity : class;

    /// <summary>
    /// Gets the database schema service.
    /// </summary>
    IDatabaseSchemaService SchemaService { get; }
}
