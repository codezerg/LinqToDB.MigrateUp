namespace LinqToDB.MigrateUp.Sql;

/// <summary>
/// SQL Server-specific implementation of ISqlQueryService.
/// </summary>
public class SqlServerQueryService : SqlQueryServiceBase
{
    /// <inheritdoc/>
    protected override string IdentifierFormat => "[{0}]";

    /// <inheritdoc/>
    public override string BuildTableExistsQuery(string tableName)
    {
        return $@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = '{tableName}'
                ) THEN 1 ELSE 0 END";
    }

    /// <inheritdoc/>
    public override string BuildIndexExistsQuery(string tableName, string indexName)
    {
        return $@"SELECT * FROM sys.indexes i
                    INNER JOIN sys.tables t ON i.object_id = t.object_id
                    WHERE t.name = '{tableName}' AND i.name = '{indexName}'";
    }

    /// <inheritdoc/>
    public override SqlQueryResult BuildGetColumnsQuery(string tableName)
    {
        var sql = $@"
                SELECT 
                    COLUMN_NAME,
                    DATA_TYPE,
                    IS_NULLABLE,
                    CHARACTER_MAXIMUM_LENGTH,
                    NUMERIC_PRECISION,
                    NUMERIC_SCALE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = '{tableName}'
                ORDER BY ORDINAL_POSITION";
        
        return new SqlQueryResult(
            sql,
            QueryResultType.SqlServerInformationSchemaColumns,
            DatabaseProvider.SqlServer
        );
    }

    /// <inheritdoc/>
    public override SqlQueryResult BuildGetIndexColumnsQuery(string tableName, string indexName)
    {
        var sql = $@"
                SELECT 
                    c.name AS COLUMN_NAME,
                    ic.is_descending_key AS IS_DESCENDING
                FROM sys.indexes i
                INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                INNER JOIN sys.tables t ON i.object_id = t.object_id
                WHERE t.name = '{tableName}' AND i.name = '{indexName}'
                ORDER BY ic.key_ordinal";
        
        return new SqlQueryResult(
            sql,
            QueryResultType.SqlServerIndexInfo,
            DatabaseProvider.SqlServer
        );
    }


    /// <inheritdoc/>
    public override string BuildAlterColumnCommand(string tableName, string columnName, string newColumnDefinition)
    {
        return $"ALTER TABLE [{tableName}] ALTER COLUMN [{columnName}] {newColumnDefinition}";
    }


    /// <inheritdoc/>
    public override string BuildDropIndexCommand(string tableName, string indexName)
    {
        return $"DROP INDEX [{indexName}] ON [{tableName}]";
    }
}