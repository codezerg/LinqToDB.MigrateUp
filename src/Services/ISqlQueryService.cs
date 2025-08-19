namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Provides database-specific SQL query generation for schema operations.
    /// </summary>
    public interface ISqlQueryService
    {
        /// <summary>
        /// Builds a SQL query to check if a table exists.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The SQL query string.</returns>
        string BuildTableExistsQuery(string tableName);

        /// <summary>
        /// Builds a SQL query to check if an index exists.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>The SQL query string.</returns>
        string BuildIndexExistsQuery(string tableName, string indexName);

        /// <summary>
        /// Builds a SQL query to get table columns information.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The SQL query string.</returns>
        string BuildGetColumnsQuery(string tableName);

        /// <summary>
        /// Builds a SQL query to get index columns information.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>The SQL query string.</returns>
        string BuildGetIndexColumnsQuery(string tableName, string indexName);

        /// <summary>
        /// Builds a SQL command to add a column to a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnDefinition">The column definition SQL.</param>
        /// <returns>The SQL command string.</returns>
        string BuildAddColumnCommand(string tableName, string columnDefinition);

        /// <summary>
        /// Builds a SQL command to alter a table column.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="newColumnDefinition">The new column definition SQL.</param>
        /// <returns>The SQL command string.</returns>
        string BuildAlterColumnCommand(string tableName, string columnName, string newColumnDefinition);

        /// <summary>
        /// Builds a SQL command to create an index.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="columnNames">The column names for the index.</param>
        /// <returns>The SQL command string.</returns>
        string BuildCreateIndexCommand(string tableName, string indexName, string[] columnNames);

        /// <summary>
        /// Builds a SQL command to drop an index.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>The SQL command string.</returns>
        string BuildDropIndexCommand(string tableName, string indexName);
    }
}