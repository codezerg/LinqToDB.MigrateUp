using LinqToDB.Data;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToDB.MigrateUp.Data;

/// <summary>
/// Concrete implementation of IDataConnectionService using LinqToDB DataConnection.
/// </summary>
public class LinqToDbDataConnectionService : IDataConnectionService
{
    private readonly DataConnection _dataConnection;

    /// <summary>
    /// Initializes a new instance of the LinqToDbDataConnectionService class.
    /// </summary>
    /// <param name="dataConnection">The LinqToDB data connection.</param>
    /// <exception cref="ArgumentNullException">Thrown when dataConnection is null.</exception>
    public LinqToDbDataConnectionService(DataConnection dataConnection)
    {
        _dataConnection = dataConnection ?? throw new ArgumentNullException(nameof(dataConnection));
    }

    /// <inheritdoc/>
    public void CreateTable<T>() where T : class
    {
        _dataConnection.CreateTable<T>();
    }

    /// <inheritdoc/>
    public IQueryable<T> GetTable<T>() where T : class
    {
        return _dataConnection.GetTable<T>();
    }

    /// <inheritdoc/>
    public int Execute(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return 0;

        // For SELECT queries that return a count, we need to use ExecuteScalar
        if (sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            var result = _dataConnection.Execute<int>(sql);
            return result;
        }
        
        // For DDL/DML operations, use Execute
        return _dataConnection.Execute(sql);
    }

    /// <inheritdoc/>
    public void BulkCopy<T>(IEnumerable<T> items) where T : class
    {
        _dataConnection.BulkCopy(items);
    }

    /// <inheritdoc/>
    public string GetEntityName<T>() where T : class
    {
        return _dataConnection.MappingSchema.GetEntityDescriptor(typeof(T)).Name.Name;
    }

    /// <inheritdoc/>
    public MappingSchema GetMappingSchema()
    {
        return _dataConnection.MappingSchema;
    }

    /// <inheritdoc/>
    public LinqToDB.IDataContext? GetDataContext()
    {
        return _dataConnection;
    }
}