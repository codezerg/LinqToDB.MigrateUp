using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.SqlQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LinqToDB.MigrateUp.Providers
{
    class SQLiteProvider : MigrationProviderBase
    {
        public SQLiteProvider(Migration migration) : base(migration)
        {
        }


        protected override bool Db_TableExists(string tableName)
        {
            var table = DataConnection.GetTable<sqlite_master>();
            var exists = table.Where(x => x.type == "table" && x.name == tableName).Any();
            return exists;
        }


        protected override IEnumerable<TableColumn> Db_GetTableColumns(string tableName)
        {
            var columns = pragma_table_info(tableName)
                .OrderBy(x => x.cid)
                .Select(x => new TableColumn(x.name, x.type, x.notnull == 0))
                .ToList();

            return columns;
        }


        protected override void Db_CreateTableColumn<Table>(string tableName, TableColumn column)
        {
            var columnName = column.ColumnName;
            var columnType = column.DataType;
            var nullable = column.IsNullable ? "NULL" : "NOT NULL";

            var sql = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnType} {nullable};";
            DataConnection.Execute(sql);
        }


        protected override void Db_AlterTableColumn(string tableName, string columnName, TableColumn newColumn)
        {
            // SQLite does not support ALTER COLUMN
        }


        protected override bool Db_TableIndexExists(string tableName, string indexName)
        {
            // Check if index already exists
            var master = DataConnection.GetTable<sqlite_master>();
            var indexExists = master.Any(x => x.type == "index" && x.name == indexName);
            return indexExists;
        }


        protected override IEnumerable<TableIndexColumn> Db_GetTableIndexColumns(string tableName, string indexName)
        {
            var columns = pragma_index_info(indexName)
                .OrderBy(x => x.seqno)
                .Select(x => new TableIndexColumn(x.name, true))
                .ToList();
            return columns;
        }


        protected override void Db_CreateTableIndex(string tableName, string indexName, IEnumerable<TableIndexColumn> columns)
        {
            // Build the CREATE INDEX SQL statement
            var columnNames = columns.Select(c => $"{c.ColumnName} {(c.IsAscending ? "ASC" : "DESC")}").ToArray();
            var sql = $@"CREATE INDEX {indexName} ON {tableName} ({string.Join(", ", columnNames)});";

            DataConnection.Execute(sql);
        }


        protected override void Db_DropTableIndex(string tableName, string indexName)
        {
            var sql = $"DROP INDEX IF EXISTS {indexName};";
            DataConnection.Execute(sql);
        }


        [Table]
        class sqlite_master
        {
            [Column] public string type { get; set; }
            [Column] public string name { get; set; }
            [Column] public string tbl_name { get; set; }
            [Column] public long rootpage { get; set; }
            [Column] public string sql { get; set; }
        }


        [Table]
        class table_info
        {
            [Column] public long cid { get; set; }
            [Column] public string name { get; set; }
            [Column] public string type { get; set; }
            [Column] public long notnull { get; set; }
            [Column] public object dflt_value { get; set; }
            [Column] public long pk { get; set; }
        }


        [Table]
        class index_info
        {
            [Column] public long seqno { get; set; }      // The order of columns within the index. Starts with 0.
            [Column] public long cid { get; set; }        // The column index from the original table.
            [Column] public string name { get; set; }    // The column name.
        }



        [Sql.TableFunction(Name = "pragma_table_info")]
        ITable<table_info> pragma_table_info(string tableName)
        {
            var methodInfo = MethodBase.GetCurrentMethod() as MethodInfo;
            return DataConnection.GetTable<table_info>(this, methodInfo, tableName);
        }


        [Sql.TableFunction(Name = "pragma_index_info")]
        ITable<index_info> pragma_index_info(string indexName)
        {
            var methodInfo = MethodBase.GetCurrentMethod() as MethodInfo;
            return DataConnection.GetTable<index_info>(this, methodInfo, indexName);
        }
    }
}
