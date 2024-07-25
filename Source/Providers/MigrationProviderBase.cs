using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.SqlProvider;
using LinqToDB.SqlQuery;

namespace LinqToDB.MigrateUp.Providers
{
    /// <summary>
    /// Provides a base implementation for migration providers.
    /// </summary>
    internal abstract class MigrationProviderBase : IMigrationProvider
    {
        /// <inheritdoc/>
        public Migration Migration { get; }

        /// <summary>
        /// Gets the data connection associated with the migration.
        /// </summary>
        protected DataConnection DataConnection => Migration.DataConnection;

        /// <summary>
        /// Gets the mapping schema associated with the data connection.
        /// </summary>
        protected MappingSchema MappingSchema => DataConnection.MappingSchema;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationProviderBase"/> class.
        /// </summary>
        /// <param name="migration">The migration associated with this provider.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="migration"/> is null.</exception>
        protected MigrationProviderBase(Migration migration)
        {
            Migration = migration ?? throw new ArgumentNullException(nameof(migration));
        }

        /// <summary>
        /// Gets the columns for a given entity type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>A collection of table columns.</returns>
        public virtual IEnumerable<TableColumn> GetEntityColumns<TEntity>()
        {
            var descriptor = MappingSchema.GetEntityDescriptor(typeof(TEntity));
            return descriptor.Columns
                .Select(x => new TableColumn(x.ColumnName, GetEntityColumnDbType(x), x.CanBeNull))
                .ToList();
        }

        private string GetEntityColumnDbType(ColumnDescriptor column)
        {
            var dataProvider = DataConnection.DataProvider;
            var sqlBuilder = dataProvider.CreateSqlBuilder(MappingSchema, DataConnection.Options);
            if (sqlBuilder == null)
            {
                throw new InvalidOperationException($"The {nameof(DataConnection)} is not configured to use the {nameof(ISqlBuilder)}");
            }

            var sqlDataType = new SqlDataType(column.GetDbDataType(true));
            return sqlBuilder.BuildDataType(new System.Text.StringBuilder(), sqlDataType).ToString();
        }

        /// <inheritdoc/>
        public void UpdateTableSchema<TEntity>() where TEntity : class
        {
            var tableName = Migration.GetEntityName<TEntity>();
            var tableExists = Db_TableExists(tableName);

            if (!tableExists)
            {
                DataConnection.CreateTable<TEntity>();
                Migration.TablesCreated.Add(tableName);
            }

            var entityColumns = GetEntityColumns<TEntity>().ToList();
            var tableColumns = Db_GetTableColumns(tableName).ToList();

            foreach (var entityColumn in entityColumns)
            {
                var tableColumn = tableColumns.FirstOrDefault(x => string.Equals(x.ColumnName, entityColumn.ColumnName, StringComparison.OrdinalIgnoreCase));
                if (tableColumn == null)
                {
                    Db_CreateTableColumn<TEntity>(tableName, entityColumn);
                }
                else if (!string.Equals(tableColumn.DataType, entityColumn.DataType, StringComparison.OrdinalIgnoreCase) || tableColumn.IsNullable != entityColumn.IsNullable)
                {
                    Db_AlterTableColumn(tableName, entityColumn.ColumnName, entityColumn);
                }
            }
        }

        /// <inheritdoc/>
        public void EnsureIndex<TEntity>(string indexName, IEnumerable<TableIndexColumn> columns) where TEntity : class
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                throw new ArgumentException("Index name cannot be null or whitespace.", nameof(indexName));
            }

            if (columns == null || !columns.Any())
            {
                throw new ArgumentException("There must be at least one column for the index.", nameof(columns));
            }

            var tableName = Migration.GetEntityName<TEntity>();
            var indexExists = Db_TableIndexExists(tableName, indexName);

            if (indexExists)
            {
                var currentIndexColumns = Db_GetTableIndexColumns(tableName, indexName);
                var areColumnsEqual = currentIndexColumns.Select(c => c.ColumnName).SequenceEqual(columns.Select(c => c.ColumnName));

                if (areColumnsEqual)
                {
                    return; // Index already exists and hasn't changed
                }

                Db_DropTableIndex(tableName, indexName);
            }

            Db_CreateTableIndex(tableName, indexName, columns);
        }

        /// <summary>
        /// Checks if a table exists in the database.
        /// </summary>
        /// <param name="tableName">The name of the table to check.</param>
        /// <returns>True if the table exists, false otherwise.</returns>
        protected abstract bool Db_TableExists(string tableName);

        /// <summary>
        /// Gets the columns of a table in the database.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A collection of table columns.</returns>
        protected abstract IEnumerable<TableColumn> Db_GetTableColumns(string tableName);

        /// <summary>
        /// Creates a new column in a table.
        /// </summary>
        /// <typeparam name="TTable">The type of the table.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="column">The column to create.</param>
        protected abstract void Db_CreateTableColumn<TTable>(string tableName, TableColumn column);

        /// <summary>
        /// Alters an existing column in a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column to alter.</param>
        /// <param name="newColumn">The new column definition.</param>
        protected abstract void Db_AlterTableColumn(string tableName, string columnName, TableColumn newColumn);

        /// <summary>
        /// Checks if an index exists on a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>True if the index exists, false otherwise.</returns>
        protected abstract bool Db_TableIndexExists(string tableName, string indexName);

        /// <summary>
        /// Gets the columns of an index on a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>A collection of table index columns.</returns>
        protected abstract IEnumerable<TableIndexColumn> Db_GetTableIndexColumns(string tableName, string indexName);

        /// <summary>
        /// Creates a new index on a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="indexName">The name of the index to create.</param>
        /// <param name="columns">The columns to include in the index.</param>
        protected abstract void Db_CreateTableIndex(string tableName, string indexName, IEnumerable<TableIndexColumn> columns);

        /// <summary>
        /// Drops an existing index from a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="indexName">The name of the index to drop.</param>
        protected abstract void Db_DropTableIndex(string tableName, string indexName);
    }
}