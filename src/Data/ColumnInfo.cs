namespace LinqToDB.MigrateUp.Data;

/// <summary>
/// Represents column information from SQLite PRAGMA table_info.
/// </summary>
internal class SqliteColumnInfo
{
    public int cid { get; set; }
    public string name { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public int notnull { get; set; }
    public string? dflt_value { get; set; }
    public int pk { get; set; }
}

/// <summary>
/// Represents column information from SQL Server INFORMATION_SCHEMA.COLUMNS.
/// </summary>
internal class SqlServerColumnInfo
{
    public string COLUMN_NAME { get; set; } = string.Empty;
    public string DATA_TYPE { get; set; } = string.Empty;
    public string IS_NULLABLE { get; set; } = string.Empty;
    public int? CHARACTER_MAXIMUM_LENGTH { get; set; }
    public int? NUMERIC_PRECISION { get; set; }
    public int? NUMERIC_SCALE { get; set; }
}