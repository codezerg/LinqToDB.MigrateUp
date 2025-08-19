using System.Linq;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// SQLite-specific implementation of ISqlQueryService.
    /// </summary>
    public class SQLiteQueryService : SqlQueryServiceBase
    {
        /// <inheritdoc/>
        protected override string IdentifierFormat => "[{0}]";
        /// <inheritdoc/>
        public override string BuildTableExistsQuery(string tableName)
        {
            return $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        }

        /// <inheritdoc/>
        public override string BuildIndexExistsQuery(string tableName, string indexName)
        {
            return $"SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name='{indexName}' AND tbl_name='{tableName}'";
        }

        /// <inheritdoc/>
        public override string BuildGetColumnsQuery(string tableName)
        {
            return $"PRAGMA table_info({tableName})";
        }

        /// <inheritdoc/>
        public override string BuildGetIndexColumnsQuery(string tableName, string indexName)
        {
            return $"PRAGMA index_info({indexName})";
        }


        /// <inheritdoc/>
        public override string BuildAlterColumnCommand(string tableName, string columnName, string newColumnDefinition)
        {
            // SQLite has limited ALTER COLUMN support, this is a simplified approach
            // In production, this might require table recreation
            return $"-- ALTER COLUMN not fully supported in SQLite for [{tableName}].[{columnName}] -> {newColumnDefinition}";
        }


        /// <inheritdoc/>
        public override string BuildDropIndexCommand(string tableName, string indexName)
        {
            return $"DROP INDEX [{indexName}]";
        }
    }
}