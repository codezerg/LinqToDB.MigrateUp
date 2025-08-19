using LinqToDB.MigrateUp.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Default implementation of IDatabaseMutationService using query services and data connection service.
    /// </summary>
    public class DatabaseMutationService : IDatabaseMutationService
    {
        private readonly IDataConnectionService _dataService;
        private readonly ISqlQueryService _queryService;

        /// <summary>
        /// Initializes a new instance of the DatabaseMutationService class.
        /// </summary>
        /// <param name="dataService">The data connection service.</param>
        /// <param name="queryService">The SQL query service.</param>
        public DatabaseMutationService(IDataConnectionService dataService, ISqlQueryService queryService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        }

        /// <inheritdoc/>
        public void CreateTableColumn<TTable>(string tableName, TableColumn column)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));
            
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var columnDefinition = BuildColumnDefinition(column);
            var command = _queryService.BuildAddColumnCommand(tableName, columnDefinition);
            _dataService.Execute(command);
        }

        /// <inheritdoc/>
        public void AlterTableColumn(string tableName, string columnName, TableColumn newColumn)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));
            
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name cannot be null or whitespace.", nameof(columnName));
            
            if (newColumn == null)
                throw new ArgumentNullException(nameof(newColumn));

            var columnDefinition = BuildColumnDefinition(newColumn);
            var command = _queryService.BuildAlterColumnCommand(tableName, columnName, columnDefinition);
            _dataService.Execute(command);
        }

        /// <inheritdoc/>
        public void CreateTableIndex(string tableName, string indexName, IEnumerable<TableIndexColumn> columns)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));
            
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));
            
            if (columns == null || !columns.Any())
                throw new ArgumentException("At least one column is required for the index.", nameof(columns));

            var columnNames = columns.Select(c => c.ColumnName).ToArray();
            var command = _queryService.BuildCreateIndexCommand(tableName, indexName, columnNames);
            _dataService.Execute(command);
        }

        /// <inheritdoc/>
        public void DropTableIndex(string tableName, string indexName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));
            
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));

            var command = _queryService.BuildDropIndexCommand(tableName, indexName);
            _dataService.Execute(command);
        }

        private string BuildColumnDefinition(TableColumn column)
        {
            var nullable = column.IsNullable ? "NULL" : "NOT NULL";
            return $"[{column.ColumnName}] {column.DataType} {nullable}";
        }
    }
}