using System;

namespace LinqToDB.MigrateUp.Sql;

/// <summary>
/// Represents the result of building a SQL query, including metadata about the expected result structure.
/// </summary>
public class SqlQueryResult
{
    /// <summary>
    /// Gets the SQL query string.
    /// </summary>
    public string Sql { get; }

    /// <summary>
    /// Gets the type of result expected from this query.
    /// </summary>
    public QueryResultType ResultType { get; }

    /// <summary>
    /// Gets the provider type this query was built for.
    /// </summary>
    public DatabaseProvider Provider { get; }

    /// <summary>
    /// Initializes a new instance of the SqlQueryResult class.
    /// </summary>
    public SqlQueryResult(string sql, QueryResultType resultType, DatabaseProvider provider)
    {
        Sql = sql ?? throw new ArgumentNullException(nameof(sql));
        ResultType = resultType;
        Provider = provider;
    }
}

/// <summary>
/// Specifies the type of result expected from a SQL query.
/// </summary>
public enum QueryResultType
{
    /// <summary>
    /// Query returns a scalar value (e.g., COUNT(*)).
    /// </summary>
    Scalar,

    /// <summary>
    /// Query returns SQLite PRAGMA table_info format.
    /// </summary>
    SqlitePragmaTableInfo,

    /// <summary>
    /// Query returns SQLite PRAGMA index_info format.
    /// </summary>
    SqlitePragmaIndexInfo,

    /// <summary>
    /// Query returns SQL Server INFORMATION_SCHEMA.COLUMNS format.
    /// </summary>
    SqlServerInformationSchemaColumns,

    /// <summary>
    /// Query returns SQL Server index information format.
    /// </summary>
    SqlServerIndexInfo,

    /// <summary>
    /// Query performs a command with no result set.
    /// </summary>
    NonQuery
}

/// <summary>
/// Specifies the database provider.
/// </summary>
public enum DatabaseProvider
{
    /// <summary>
    /// SQLite database.
    /// </summary>
    SQLite,

    /// <summary>
    /// SQL Server database.
    /// </summary>
    SqlServer,

    /// <summary>
    /// PostgreSQL database.
    /// </summary>
    PostgreSQL,

    /// <summary>
    /// MySQL database.
    /// </summary>
    MySQL,

    /// <summary>
    /// Unknown or generic provider.
    /// </summary>
    Unknown
}