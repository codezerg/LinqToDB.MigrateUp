namespace LinqToDB.MigrateUp.Sql;

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
    public override SqlQueryResult BuildGetColumnsQuery(string tableName)
    {
        return new SqlQueryResult(
            $"PRAGMA table_info({tableName})",
            QueryResultType.SqlitePragmaTableInfo,
            DatabaseProvider.SQLite
        );
    }

    /// <inheritdoc/>
    public override SqlQueryResult BuildGetIndexColumnsQuery(string tableName, string indexName)
    {
        return new SqlQueryResult(
            $"PRAGMA index_info({indexName})",
            QueryResultType.SqlitePragmaIndexInfo,
            DatabaseProvider.SQLite
        );
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