using System.Linq;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// SQLite-specific implementation of ISqlQueryService.
    /// </summary>
    public class SQLiteQueryService : ISqlQueryService
    {
        /// <inheritdoc/>
        public string BuildTableExistsQuery(string tableName)
        {
            return $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        }

        /// <inheritdoc/>
        public string BuildIndexExistsQuery(string tableName, string indexName)
        {
            return $"SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name='{indexName}' AND tbl_name='{tableName}'";
        }

        /// <inheritdoc/>
        public string BuildGetColumnsQuery(string tableName)
        {
            return $"PRAGMA table_info({tableName})";
        }

        /// <inheritdoc/>
        public string BuildGetIndexColumnsQuery(string tableName, string indexName)
        {
            return $"PRAGMA index_info({indexName})";
        }

        /// <inheritdoc/>
        public string BuildAddColumnCommand(string tableName, string columnDefinition)
        {
            return $"ALTER TABLE [{tableName}] ADD COLUMN {columnDefinition}";
        }

        /// <inheritdoc/>
        public string BuildAlterColumnCommand(string tableName, string columnName, string newColumnDefinition)
        {
            // SQLite has limited ALTER COLUMN support, this is a simplified approach
            // In production, this might require table recreation
            return $"-- ALTER COLUMN not fully supported in SQLite for [{tableName}].[{columnName}] -> {newColumnDefinition}";
        }

        /// <inheritdoc/>
        public string BuildCreateIndexCommand(string tableName, string indexName, string[] columnNames)
        {
            var columns = string.Join(", ", columnNames.Select(c => $"[{c}]"));
            return $"CREATE INDEX [{indexName}] ON [{tableName}] ({columns})";
        }

        /// <inheritdoc/>
        public string BuildDropIndexCommand(string tableName, string indexName)
        {
            return $"DROP INDEX [{indexName}]";
        }
    }
}