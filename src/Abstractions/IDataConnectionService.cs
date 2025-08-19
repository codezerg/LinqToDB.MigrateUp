using System;
using LinqToDB.MigrateUp.Abstractions;
using System.Collections.Generic;
using System.Linq;
using LinqToDB.Mapping;

namespace LinqToDB.MigrateUp.Abstractions;

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
    /// Gets access to the underlying data context for advanced operations.
    /// </summary>
    /// <returns>The data context.</returns>
    LinqToDB.IDataContext? GetDataContext();

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

    /// <summary>
    /// Gets the mapping schema associated with this data connection.
    /// </summary>
    /// <returns>The mapping schema.</returns>
    MappingSchema GetMappingSchema();

    /// <summary>
    /// Executes a SQL query and returns a list of dynamic results.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>The query results.</returns>
    IEnumerable<dynamic> Query(string sql);

    /// <summary>
    /// Executes a SQL query and returns a list of strongly-typed results.
    /// </summary>
    /// <typeparam name="T">The type to map the results to.</typeparam>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>The query results.</returns>
    IEnumerable<T> Query<T>(string sql) where T : class;
}