using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LinqToDB.MigrateUp.Services;

namespace LinqToDB.MigrateUp.Tests.Testing
{
    /// <summary>
    /// Mock implementation of IMigrationStateManager for testing purposes.
    /// </summary>
    public class MockMigrationStateManager : IMigrationStateManager
    {
        private readonly HashSet<string> _tablesCreated = new HashSet<string>();
        private readonly HashSet<string> _indexesCreated = new HashSet<string>();

        /// <summary>
        /// Gets or sets whether events should be fired when marking items as created.
        /// </summary>
        public bool FireEvents { get; set; } = true;

        /// <summary>
        /// Gets the set of tables that have been marked as created.
        /// </summary>
        public IReadOnlyCollection<string> TablesCreated => _tablesCreated;

        /// <summary>
        /// Gets the set of indexes that have been marked as created.
        /// </summary>
        public IReadOnlyCollection<string> IndexesCreated => _indexesCreated;

        /// <inheritdoc/>
        public event EventHandler<string>? TableCreated;

        /// <inheritdoc/>
        public event EventHandler<string>? IndexCreated;

        /// <summary>
        /// Clears all tracking information.
        /// </summary>
        public void Reset()
        {
            _tablesCreated.Clear();
            _indexesCreated.Clear();
        }

        /// <inheritdoc/>
        public void MarkTableCreated(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));

            if (_tablesCreated.Add(tableName) && FireEvents)
            {
                TableCreated?.Invoke(this, tableName);
            }
        }

        /// <inheritdoc/>
        public void MarkIndexCreated(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));

            if (_indexesCreated.Add(indexName) && FireEvents)
            {
                IndexCreated?.Invoke(this, indexName);
            }
        }

        /// <inheritdoc/>
        public bool IsTableCreated(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return false;

            return _tablesCreated.Contains(tableName);
        }

        /// <inheritdoc/>
        public bool IsIndexCreated(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                return false;

            return _indexesCreated.Contains(indexName);
        }
    }
}