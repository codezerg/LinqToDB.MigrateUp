using System;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.MigrateUp.Validation;

namespace LinqToDB.MigrateUp.Sql
{
    /// <summary>
    /// Base class for SQL query services with common patterns.
    /// </summary>
    public abstract class SqlQueryServiceBase : ISqlQueryService
    {
        /// <summary>
        /// Gets the SQL format for identifiers (e.g., brackets for SQL Server, quotes for others).
        /// </summary>
        protected abstract string IdentifierFormat { get; }

        /// <summary>
        /// Formats an identifier for SQL queries.
        /// </summary>
        protected virtual string FormatIdentifier(string identifier)
        {
            ValidationHelper.ValidateSqlIdentifier(identifier, nameof(identifier));
            return string.Format(IdentifierFormat, identifier);
        }

        /// <inheritdoc/>
        public abstract string BuildTableExistsQuery(string tableName);

        /// <inheritdoc/>
        public abstract string BuildIndexExistsQuery(string tableName, string indexName);

        /// <inheritdoc/>
        public abstract string BuildGetColumnsQuery(string tableName);

        /// <inheritdoc/>
        public abstract string BuildGetIndexColumnsQuery(string tableName, string indexName);

        /// <inheritdoc/>
        public virtual string BuildAddColumnCommand(string tableName, string columnDefinition)
        {
            ValidationHelper.ValidateSqlIdentifier(tableName, nameof(tableName));
            ValidationHelper.ValidateNotNullOrWhiteSpace(columnDefinition, nameof(columnDefinition));
            
            return $"ALTER TABLE {FormatIdentifier(tableName)} ADD {columnDefinition}";
        }

        /// <inheritdoc/>
        public abstract string BuildAlterColumnCommand(string tableName, string columnName, string newColumnDefinition);

        /// <inheritdoc/>
        public virtual string BuildCreateIndexCommand(string tableName, string indexName, string[] columnNames)
        {
            ValidationHelper.ValidateSqlIdentifier(tableName, nameof(tableName));
            ValidationHelper.ValidateSqlIdentifier(indexName, nameof(indexName));
            
            if (columnNames == null || columnNames.Length == 0)
            {
                throw new ArgumentException("At least one column is required for the index.", nameof(columnNames));
            }

            foreach (var columnName in columnNames)
            {
                ValidationHelper.ValidateSqlIdentifier(columnName, nameof(columnName));
            }

            var formattedColumns = string.Join(", ", Array.ConvertAll(columnNames, FormatIdentifier));
            return $"CREATE INDEX {FormatIdentifier(indexName)} ON {FormatIdentifier(tableName)} ({formattedColumns})";
        }

        /// <inheritdoc/>
        public abstract string BuildDropIndexCommand(string tableName, string indexName);
    }
}