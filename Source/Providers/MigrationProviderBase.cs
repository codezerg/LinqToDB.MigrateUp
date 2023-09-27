using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Data;
using LinqToDB.SqlProvider;
using LinqToDB.SqlQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToDB.MigrateUp.Providers
{
    abstract class MigrationProviderBase : IMigrationProvider
    {
        public Migration Migration { get; }
        public DataConnection DataConnection => Migration.DataConnection;
        public MappingSchema MappingSchema => DataConnection.MappingSchema;


        public MigrationProviderBase(Migration migration)
        {
            Migration = migration;
        }


        public virtual void Run(MigrationProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            foreach (var task in profile.Tasks)
            {
                task.Run(this);
            }
        }


        public virtual IEnumerable<TableColumn> GetEntityColumns<TEntity>()
        {
            var descriptor = MappingSchema.GetEntityDescriptor(typeof(TEntity));
            var columns = descriptor.Columns
                .Select(x => new TableColumn(x.ColumnName, GetEntityColumnDbType(x), x.CanBeNull))
                .ToList();

            return columns;
        }


        private string GetEntityColumnDbType(ColumnDescriptor column)
        {
            var dataProvider = DataConnection.DataProvider;
            var mappingSchema = DataConnection.MappingSchema;
            var dataOptions = DataConnection.Options;

            var sqlBuilder = dataProvider.CreateSqlBuilder(mappingSchema, dataOptions);
            if (sqlBuilder == null)
            {
                throw new InvalidOperationException($"The {nameof(DataConnection)} is not configured to use the {nameof(ISqlBuilder)}");
            }

            var sqlDataType = new SqlDataType(column.GetDbDataType(true));
            var sb = sqlBuilder.BuildDataType(new StringBuilder(), sqlDataType);

            return sb.ToString();
        }


        public abstract bool Db_TableExists(string tableName);
        public abstract IEnumerable<TableColumn> Db_GetTableColumns(string tableName);
        public abstract void Db_CreateTableColumn<Table>(string tableName, TableColumn column);


        public void CreateTable<Table>()
        {
            var tableName = Migration.GetEntityName<Table>();
            var tableExists = Db_TableExists(tableName);

            if (tableExists == false)
            {
                DataConnection.CreateTable<Table>();
                Migration.TablesCreated.Add(tableName);
            }

            var entityColumns = GetEntityColumns<Table>().ToList();
            var tableColumns = Db_GetTableColumns(tableName).ToList();

            foreach (var entityColumn in entityColumns)
            {
                var tableColumn = tableColumns.FirstOrDefault(x => x.ColumnName == entityColumn.ColumnName);
                if (tableColumn == null)
                {
                    Db_CreateTableColumn<Table>(tableName, entityColumn);
                }
            }
        }


        public abstract bool Db_TableIndexExists(string tableName, string indexName);
        public abstract IEnumerable<TableIndexColumn> Db_GetTableIndexColumns(string tableName, string indexName);
        public abstract void Db_CreateTableIndex(string tableName, string indexName, IEnumerable<TableIndexColumn> columns);
        public abstract void Db_DropTableIndex(string tableName, string indexName);


        public void CreateIndex<Table>(string indexName, IEnumerable<TableIndexColumn> columns)
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                throw new ArgumentException("Index name cannot be null or empty.", nameof(indexName));
            }

            if (columns == null || !columns.Any())
            {
                throw new ArgumentException("There must be at least one column for the index.", nameof(columns));
            }

            var tableName = Migration.GetEntityName<Table>();
            var indexExists = Db_TableIndexExists(tableName, indexName);

            if (indexExists)
            {
                // If the index definition has changed, recreate the index
                var currentIndexColumns = Db_GetTableIndexColumns(tableName, indexName);
                var areColumnsEqual = currentIndexColumns.Select(c => c.ColumnName).SequenceEqual(columns.Select(c => c.ColumnName));

                if (areColumnsEqual)
                    return; // Index already exists and hasn't changed

                Db_DropTableIndex(tableName, indexName);
            }

            Db_CreateTableIndex(tableName, indexName, columns);
        }
    }
}
