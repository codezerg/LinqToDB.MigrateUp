using System.Linq;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// SQL Server-specific implementation of ISqlQueryService.
    /// </summary>
    public class SqlServerQueryService : ISqlQueryService
    {
        /// <inheritdoc/>
        public string BuildTableExistsQuery(string tableName)
        {
            return $@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = '{tableName}'
                ) THEN 1 ELSE 0 END";
        }

        /// <inheritdoc/>
        public string BuildIndexExistsQuery(string tableName, string indexName)
        {
            return $@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM sys.indexes i
                    INNER JOIN sys.tables t ON i.object_id = t.object_id
                    WHERE t.name = '{tableName}' AND i.name = '{indexName}'
                ) THEN 1 ELSE 0 END";
        }

        /// <inheritdoc/>
        public string BuildGetColumnsQuery(string tableName)
        {
            return $@"
                SELECT 
                    COLUMN_NAME,
                    DATA_TYPE,
                    IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = '{tableName}'
                ORDER BY ORDINAL_POSITION";
        }

        /// <inheritdoc/>
        public string BuildGetIndexColumnsQuery(string tableName, string indexName)
        {
            return $@"
                SELECT 
                    c.name AS COLUMN_NAME,
                    ic.is_descending_key AS IS_DESCENDING
                FROM sys.indexes i
                INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                INNER JOIN sys.tables t ON i.object_id = t.object_id
                WHERE t.name = '{tableName}' AND i.name = '{indexName}'
                ORDER BY ic.key_ordinal";
        }

        /// <inheritdoc/>
        public string BuildAddColumnCommand(string tableName, string columnDefinition)
        {
            return $"ALTER TABLE [{tableName}] ADD {columnDefinition}";
        }

        /// <inheritdoc/>
        public string BuildAlterColumnCommand(string tableName, string columnName, string newColumnDefinition)
        {
            return $"ALTER TABLE [{tableName}] ALTER COLUMN [{columnName}] {newColumnDefinition}";
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
            return $"DROP INDEX [{indexName}] ON [{tableName}]";
        }
    }
}