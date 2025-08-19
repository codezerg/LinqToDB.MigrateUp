using LinqToDB.MigrateUp.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToDB.MigrateUp.Services.Testing
{
    /// <summary>
    /// Mock implementation of IDatabaseSchemaService for testing purposes.
    /// </summary>
    public class MockDatabaseSchemaService : IDatabaseSchemaService
    {
        private readonly Dictionary<string, bool> _tableExists = new Dictionary<string, bool>();
        private readonly Dictionary<string, Dictionary<string, bool>> _indexExists = new Dictionary<string, Dictionary<string, bool>>();
        private readonly Dictionary<string, List<TableColumn>> _tableColumns = new Dictionary<string, List<TableColumn>>();
        private readonly Dictionary<string, Dictionary<string, List<TableIndexColumn>>> _indexColumns = new Dictionary<string, Dictionary<string, List<TableIndexColumn>>>();

        /// <summary>
        /// Sets whether a table exists.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="exists">Whether the table exists.</param>
        public void SetTableExists(string tableName, bool exists)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));

            _tableExists[tableName] = exists;
        }

        /// <summary>
        /// Sets whether an index exists.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="exists">Whether the index exists.</param>
        public void SetIndexExists(string tableName, string indexName, bool exists)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));
            
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));

            if (!_indexExists.ContainsKey(tableName))
                _indexExists[tableName] = new Dictionary<string, bool>();

            _indexExists[tableName][indexName] = exists;
        }

        /// <summary>
        /// Sets the columns for a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columns">The columns for the table.</param>
        public void SetTableColumns(string tableName, IEnumerable<TableColumn> columns)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));

            _tableColumns[tableName] = columns?.ToList() ?? new List<TableColumn>();
        }

        /// <summary>
        /// Sets the columns for an index.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="columns">The columns for the index.</param>
        public void SetIndexColumns(string tableName, string indexName, IEnumerable<TableIndexColumn> columns)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));
            
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));

            if (!_indexColumns.ContainsKey(tableName))
                _indexColumns[tableName] = new Dictionary<string, List<TableIndexColumn>>();

            _indexColumns[tableName][indexName] = columns?.ToList() ?? new List<TableIndexColumn>();
        }

        /// <summary>
        /// Clears all mock configuration.
        /// </summary>
        public void Reset()
        {
            _tableExists.Clear();
            _indexExists.Clear();
            _tableColumns.Clear();
            _indexColumns.Clear();
        }

        /// <inheritdoc/>
        public bool TableExists(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return false;

            return _tableExists.TryGetValue(tableName, out var exists) && exists;
        }

        /// <inheritdoc/>
        public bool IndexExists(string tableName, string indexName)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(indexName))
                return false;

            return _indexExists.TryGetValue(tableName, out var indexes) &&
                   indexes.TryGetValue(indexName, out var exists) &&
                   exists;
        }

        /// <inheritdoc/>
        public IEnumerable<TableColumn> GetTableColumns(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return Enumerable.Empty<TableColumn>();

            return _tableColumns.TryGetValue(tableName, out var columns) 
                ? columns 
                : Enumerable.Empty<TableColumn>();
        }

        /// <inheritdoc/>
        public IEnumerable<TableIndexColumn> GetIndexColumns(string tableName, string indexName)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(indexName))
                return Enumerable.Empty<TableIndexColumn>();

            return _indexColumns.TryGetValue(tableName, out var tableIndexes) &&
                   tableIndexes.TryGetValue(indexName, out var columns)
                ? columns
                : Enumerable.Empty<TableIndexColumn>();
        }
    }
}