using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.MigrateUp.Data;

namespace LinqToDB.MigrateUp.Tests.Testing
{
    /// <summary>
    /// Mock implementation of IDataConnectionService for testing purposes.
    /// </summary>
    public class MockDataConnectionService : IDataConnectionService
    {
        private readonly Dictionary<Type, List<object>> _tables = new Dictionary<Type, List<object>>();
        private readonly List<string> _executedSql = new List<string>();

        /// <summary>
        /// Gets the list of tables that have been created.
        /// </summary>
        public List<string> CreatedTables { get; } = new List<string>();

        /// <summary>
        /// Gets the data that has been bulk copied, organized by entity type.
        /// </summary>
        public Dictionary<Type, List<object>> BulkCopiedData { get; } = new Dictionary<Type, List<object>>();

        /// <summary>
        /// Gets the list of SQL commands that have been executed.
        /// </summary>
        public List<string> ExecutedSql => new List<string>(_executedSql);

        /// <summary>
        /// Gets the list of executed commands (alias for ExecutedSql).
        /// </summary>
        public List<string> GetExecutedCommands() => ExecutedSql;

        /// <summary>
        /// Gets or sets the return value for Execute operations.
        /// </summary>
        public int ExecuteReturnValue { get; set; } = 1;

        /// <summary>
        /// Sets up mock data for a table.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="data">The mock data to return.</param>
        public void SetupTableData<T>(IEnumerable<T> data) where T : class
        {
            var entityType = typeof(T);
            if (!_tables.ContainsKey(entityType))
                _tables[entityType] = new List<object>();

            _tables[entityType].Clear();
            _tables[entityType].AddRange(data.Cast<object>());
        }

        /// <summary>
        /// Clears all mock data and tracking information.
        /// </summary>
        public void Reset()
        {
            _tables.Clear();
            _executedSql.Clear();
            CreatedTables.Clear();
            BulkCopiedData.Clear();
            ExecuteReturnValue = 1;
        }

        /// <inheritdoc/>
        public void CreateTable<T>() where T : class
        {
            var tableName = GetEntityName<T>();
            CreatedTables.Add(tableName);

            var entityType = typeof(T);
            if (!_tables.ContainsKey(entityType))
                _tables[entityType] = new List<object>();
        }

        /// <inheritdoc/>
        public IQueryable<T> GetTable<T>() where T : class
        {
            var entityType = typeof(T);
            if (!_tables.ContainsKey(entityType))
                _tables[entityType] = new List<object>();

            return _tables[entityType].Cast<T>().AsQueryable();
        }

        /// <inheritdoc/>
        public int Execute(string sql)
        {
            if (!string.IsNullOrWhiteSpace(sql))
                _executedSql.Add(sql);

            return ExecuteReturnValue;
        }

        /// <inheritdoc/>
        public void BulkCopy<T>(IEnumerable<T> items) where T : class
        {
            var entityType = typeof(T);
            if (!BulkCopiedData.ContainsKey(entityType))
                BulkCopiedData[entityType] = new List<object>();

            BulkCopiedData[entityType].AddRange(items.Cast<object>());

            // Also add to the queryable table data
            if (!_tables.ContainsKey(entityType))
                _tables[entityType] = new List<object>();

            _tables[entityType].AddRange(items.Cast<object>());
        }

        /// <inheritdoc/>
        public string GetEntityName<T>() where T : class
        {
            // Check for [Table] attribute to get the correct table name
            var tableAttribute = typeof(T).GetCustomAttributes(typeof(LinqToDB.Mapping.TableAttribute), false)
                .Cast<LinqToDB.Mapping.TableAttribute>()
                .FirstOrDefault();
            
            return tableAttribute?.Name ?? typeof(T).Name;
        }

        /// <inheritdoc/>
        public MappingSchema GetMappingSchema()
        {
            // Return a default mapping schema for testing
            return MappingSchema.Default;
        }

        /// <inheritdoc/>
        public LinqToDB.IDataContext? GetDataContext()
        {
            // For testing, return null as we don't have a real data context
            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<dynamic> Query(string sql)
        {
            _executedSql.Add(sql);
            // Return empty for testing - could be enhanced to return mock data based on SQL
            return Enumerable.Empty<dynamic>();
        }

        /// <inheritdoc/>
        public IEnumerable<T> Query<T>(string sql) where T : class
        {
            _executedSql.Add(sql);
            // Return empty for testing - could be enhanced to return mock data based on SQL
            return Enumerable.Empty<T>();
        }
    }
}