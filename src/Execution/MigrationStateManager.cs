using System;
using LinqToDB.MigrateUp.Abstractions;
using System.Collections.Concurrent;

namespace LinqToDB.MigrateUp.Execution;

/// <summary>
/// Thread-safe implementation of IMigrationStateManager using concurrent collections.
/// </summary>
public class MigrationStateManager : IMigrationStateManager
{
    private readonly ConcurrentDictionary<string, bool> _tablesCreated = new ConcurrentDictionary<string, bool>();
    private readonly ConcurrentDictionary<string, bool> _indexesCreated = new ConcurrentDictionary<string, bool>();

    /// <inheritdoc/>
    public event EventHandler<string>? TableCreated;

    /// <inheritdoc/>
    public event EventHandler<string>? IndexCreated;

    /// <inheritdoc/>
    public void MarkTableCreated(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));

        if (_tablesCreated.TryAdd(tableName, true))
        {
            TableCreated?.Invoke(this, tableName);
        }
    }

    /// <inheritdoc/>
    public void MarkIndexCreated(string indexName)
    {
        if (string.IsNullOrWhiteSpace(indexName))
            throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));

        if (_indexesCreated.TryAdd(indexName, true))
        {
            IndexCreated?.Invoke(this, indexName);
        }
    }

    /// <inheritdoc/>
    public bool IsTableCreated(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return false;

        return _tablesCreated.ContainsKey(tableName);
    }

    /// <inheritdoc/>
    public bool IsIndexCreated(string indexName)
    {
        if (string.IsNullOrWhiteSpace(indexName))
            return false;

        return _indexesCreated.ContainsKey(indexName);
    }

    /// <inheritdoc/>
    public void Reset()
    {
        _tablesCreated.Clear();
        _indexesCreated.Clear();
    }
}