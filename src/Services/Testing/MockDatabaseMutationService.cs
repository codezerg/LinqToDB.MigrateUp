using LinqToDB.MigrateUp.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToDB.MigrateUp.Services.Testing
{
    /// <summary>
    /// Mock implementation of IDatabaseMutationService for testing purposes.
    /// </summary>
    public class MockDatabaseMutationService : IDatabaseMutationService
    {
        /// <summary>
        /// Gets the list of columns that have been created.
        /// </summary>
        public List<(string TableName, TableColumn Column)> CreatedColumns { get; } = new List<(string, TableColumn)>();

        /// <summary>
        /// Gets the list of columns that have been altered.
        /// </summary>
        public List<(string TableName, string ColumnName, TableColumn NewColumn)> AlteredColumns { get; } = new List<(string, string, TableColumn)>();

        /// <summary>
        /// Gets the list of indexes that have been created.
        /// </summary>
        public List<(string TableName, string IndexName, IEnumerable<TableIndexColumn> Columns)> CreatedIndexes { get; } = new List<(string, string, IEnumerable<TableIndexColumn>)>();

        /// <summary>
        /// Gets the list of indexes that have been dropped.
        /// </summary>
        public List<(string TableName, string IndexName)> DroppedIndexes { get; } = new List<(string, string)>();

        /// <summary>
        /// Gets or sets whether operations should throw exceptions (for testing error scenarios).
        /// </summary>
        public bool ThrowOnOperations { get; set; } = false;

        /// <summary>
        /// Gets or sets the exception message to throw when ThrowOnOperations is true.
        /// </summary>
        public string ExceptionMessage { get; set; } = "Mock database operation failed";

        /// <summary>
        /// Clears all tracking information.
        /// </summary>
        public void Reset()
        {
            CreatedColumns.Clear();
            AlteredColumns.Clear();
            CreatedIndexes.Clear();
            DroppedIndexes.Clear();
            ThrowOnOperations = false;
            ExceptionMessage = "Mock database operation failed";
        }

        /// <inheritdoc/>
        public void CreateTableColumn<TTable>(string tableName, TableColumn column)
        {
            if (ThrowOnOperations)
                throw new InvalidOperationException(ExceptionMessage);

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));

            if (column == null)
                throw new ArgumentNullException(nameof(column));

            CreatedColumns.Add((tableName, column));
        }

        /// <inheritdoc/>
        public void AlterTableColumn(string tableName, string columnName, TableColumn newColumn)
        {
            if (ThrowOnOperations)
                throw new InvalidOperationException(ExceptionMessage);

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));

            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name cannot be null or whitespace.", nameof(columnName));

            if (newColumn == null)
                throw new ArgumentNullException(nameof(newColumn));

            AlteredColumns.Add((tableName, columnName, newColumn));
        }

        /// <inheritdoc/>
        public void CreateTableIndex(string tableName, string indexName, IEnumerable<TableIndexColumn> columns)
        {
            if (ThrowOnOperations)
                throw new InvalidOperationException(ExceptionMessage);

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));

            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));

            if (columns == null || !columns.Any())
                throw new ArgumentException("At least one column is required for the index.", nameof(columns));

            CreatedIndexes.Add((tableName, indexName, columns.ToList()));
        }

        /// <inheritdoc/>
        public void DropTableIndex(string tableName, string indexName)
        {
            if (ThrowOnOperations)
                throw new InvalidOperationException(ExceptionMessage);

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));

            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));

            DroppedIndexes.Add((tableName, indexName));
        }
    }
}