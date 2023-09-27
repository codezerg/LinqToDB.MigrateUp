using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Data;
using LinqToDB.SqlQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LinqToDB.MigrateUp.Providers
{
    class SqlServerProvider : MigrationProviderBase
    {
        public SqlServerProvider(Migration migration) : base(migration)
        {
        }


        public override bool Db_TableExists(string tableName)
        {
            var tableCount = DataConnection.Execute<int>($"SELECT COUNT(*) FROM sys.tables WHERE name = N'{tableName}'");
            return tableCount > 0;
        }


        public override bool Db_TableIndexExists(string tableName, string indexName)
        {
            var indexCount = DataConnection.Execute<int>($"SELECT COUNT(*) FROM sys.indexes WHERE name = N'{indexName}' AND object_id = OBJECT_ID(N'{tableName}')");
            return indexCount > 0;
        }


        public override IEnumerable<TableColumn> Db_GetTableColumns(string tableName)
        {
            var db_columns = DataConnection.GetTable<INFORMATION_SCHEMA_COLUMNS>()
                .Where(x => x.TABLE_NAME == tableName)
                .ToList();

            var result = db_columns
                .Select(x => new TableColumn(x.COLUMN_NAME, x.DATA_TYPE, x.IS_NULLABLE == "YES"))
                .ToList();

            return result;
        }


        public override void Db_DropTableIndex(string tableName, string indexName)
        {
            DataConnection.Execute($"DROP INDEX [{indexName}] ON [{tableName}]");
        }


        public override void Db_CreateTableColumn<Table>(string tableName, TableColumn entityColumn)
        {
            var nullableSql = entityColumn.IsNullable ? "NULL" : "NOT NULL";
            var alterTableSql = $"ALTER TABLE {tableName} ADD {entityColumn.ColumnName} {entityColumn.DataType} {nullableSql}";

            DataConnection.Execute(alterTableSql);
        }


        public override void Db_CreateTableIndex(string tableName, string indexName, IEnumerable<TableIndexColumn> columns)
        {
            var columnsSql = string.Join(", ", columns.Select(c => $"{c.ColumnName} {(c.IsAscending ? "ASC" : "DESC")}"));

            var createIndexSql = $"CREATE INDEX {indexName} ON {tableName} ({columnsSql})";

            DataConnection.Execute(createIndexSql);
        }


        public override IEnumerable<TableIndexColumn> Db_GetTableIndexColumns(string tableName, string indexName)
        {
            var query = from i in DataConnection.GetTable<sys_indexes>()
                        join ic in DataConnection.GetTable<sys_index_columns>() on new { i.object_id, i.index_id } equals new { ic.object_id, ic.index_id }
                        join c in DataConnection.GetTable<sys_columns>() on new { ic.object_id, ic.column_id } equals new { c.object_id, c.column_id }
                        where i.name == indexName && OBJECT_NAME(i.object_id) == tableName
                        select new TableIndexColumn(c.name, ic.is_descending_key == 0); // assuming is_descending_key = "0" means ASC

            return query.ToList();
        }





        [Sql.Function(Name = "OBJECT_NAME", ServerSideOnly = true)]
        static string OBJECT_NAME(int object_id)
        {
            throw new InvalidOperationException();
        }


        [Table(Schema = "sys", Name = "columns")]
        class sys_columns
        {
            [Column] public int object_id { get; set; }
            [Column] public int column_id { get; set; }
            [Column] public string name { get; set; }
        }



        [Table(Schema = "sys", Name = "index_columns")]
        class sys_index_columns
        {
            [Column] public int object_id { get; set; }
            [Column] public int index_id { get; set; }
            [Column] public int index_column_id { get; set; }
            [Column] public int column_id { get; set; }
            [Column] public int is_descending_key { get; set; }
        }


        [Table(Schema = "sys", Name = "indexes")]
        class sys_indexes
        {
            [Column] public int object_id { get; set; }
            [Column] public int index_id { get; set; }
            [Column] public string name { get; set; }
            [Column] public string type_desc { get; set; }
        }


        [Table(Schema = "INFORMATION_SCHEMA", Name = "COLUMNS")]
        class INFORMATION_SCHEMA_COLUMNS
        {
            [Column] public string TABLE_NAME { get; set; }
            [Column] public string COLUMN_NAME { get; set; }
            [Column] public string DATA_TYPE { get; set; }
            [Column] public string IS_NULLABLE { get; set; }
        }
    }
}
