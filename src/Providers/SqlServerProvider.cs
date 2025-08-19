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
    class SqlServerProvider : MigrationProviderBase
    {
        private readonly SqlServerColumnAlterationChecker _alterationChecker;


        public SqlServerProvider(Migration migration) : base(migration)
        {
            _alterationChecker = new SqlServerColumnAlterationChecker();
        }


        protected override bool Db_TableExists(string tableName)
        {
            var tableCount = DataConnection.Execute<int>($"SELECT COUNT(*) FROM sys.tables WHERE name = N'{tableName}'");
            return tableCount > 0;
        }


        protected override bool Db_TableIndexExists(string tableName, string indexName)
        {
            var indexCount = DataConnection.Execute<int>($"SELECT COUNT(*) FROM sys.indexes WHERE name = N'{indexName}' AND object_id = OBJECT_ID(N'{tableName}')");
            return indexCount > 0;
        }


        protected override IEnumerable<TableColumn> Db_GetTableColumns(string tableName)
        {
            var db_columns = DataConnection.GetTable<INFORMATION_SCHEMA_COLUMNS>()
                .Where(x => x.TABLE_NAME == tableName)
                .ToList();

            var result = db_columns
                .Select(x => new TableColumn(x.COLUMN_NAME, CreateSqlServerDataType(x).ToString(), x.IS_NULLABLE == "YES"))
                .ToList();

            return result;
        }


        private SqlServerDataType CreateSqlServerDataType(INFORMATION_SCHEMA_COLUMNS column)
        {
            string maxLength = column.CHARACTER_MAXIMUM_LENGTH == -1 ? "max" :
                               column.CHARACTER_MAXIMUM_LENGTH.HasValue ? column.CHARACTER_MAXIMUM_LENGTH.ToString() :
                               null;

            return new SqlServerDataType(
                column.DATA_TYPE,
                maxLength,
                column.NUMERIC_PRECISION ?? column.DATETIME_PRECISION,
                column.NUMERIC_SCALE
            );
        }


        protected override void Db_DropTableIndex(string tableName, string indexName)
        {
            DataConnection.Execute($"DROP INDEX [{indexName}] ON [{tableName}]");
        }


        protected override void Db_CreateTableColumn<Table>(string tableName, TableColumn entityColumn)
        {
            var nullableSql = entityColumn.IsNullable ? "NULL" : "NOT NULL";
            var alterTableSql = $"ALTER TABLE {tableName} ADD {entityColumn.ColumnName} {entityColumn.DataType} {nullableSql}";

            DataConnection.Execute(alterTableSql);
        }


        protected override void Db_AlterTableColumn(string tableName, string columnName, TableColumn newColumn)
        {
            var currentColumn = Db_GetTableColumns(tableName).FirstOrDefault(c => c.ColumnName == columnName);
            if (currentColumn == null)
            {
                //throw new InvalidOperationException($"Column {columnName} does not exist in table {tableName}");
                return;
            }

            var currentType = SqlServerDataType.FromString(currentColumn.DataType);
            var newType = SqlServerDataType.FromString(newColumn.DataType);

            if (_alterationChecker.CanSafelyAlterColumn(currentType, newType))
            {
                if (IsCompatibleNullabilityChange(currentColumn, newColumn))
                {
                    var nullableSql = newColumn.IsNullable ? "NULL" : "NOT NULL";
                    var alterColumnSql = $"ALTER TABLE {tableName} ALTER COLUMN {columnName} {newType} {nullableSql}";
                    DataConnection.Execute(alterColumnSql);
                    return;
                }
            }

            // Handle the case when the alteration is not safe
            // You might want to log this or throw an exception

            //throw new InvalidOperationException($"Cannot safely alter column {columnName} from {currentColumn.DataType} to {newColumn.DataType}");
        }


        private bool IsCompatibleNullabilityChange(TableColumn currentColumn, TableColumn newColumn)
        {
            // Always allow changing from nullable to non-nullable
            if (currentColumn.IsNullable && !newColumn.IsNullable)
            {
                return true;
            }

            // Allow changing from non-nullable to nullable only if explicitly specified
            return currentColumn.IsNullable == newColumn.IsNullable;
        }


        protected override void Db_CreateTableIndex(string tableName, string indexName, IEnumerable<TableIndexColumn> columns)
        {
            var columnsSql = string.Join(", ", columns.Select(c => $"{c.ColumnName} {(c.IsAscending ? "ASC" : "DESC")}"));

            var createIndexSql = $"CREATE INDEX [{indexName}] ON [{tableName}] ({columnsSql})";

            DataConnection.Execute(createIndexSql);
        }


        protected override IEnumerable<TableIndexColumn> Db_GetTableIndexColumns(string tableName, string indexName)
        {
            var query = from i in DataConnection.GetTable<sys_indexes>()
                        join ic in DataConnection.GetTable<sys_index_columns>() on new { i.object_id, i.index_id } equals new { ic.object_id, ic.index_id }
                        join c in DataConnection.GetTable<sys_columns>() on new { ic.object_id, ic.column_id } equals new { c.object_id, c.column_id }
                        where i.name == indexName && OBJECT_NAME(i.object_id) == tableName
                        select new TableIndexColumn(c.name, ic.is_descending_key == 0); // is_descending_key = "0" means ASC

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
            [Column] public int? CHARACTER_MAXIMUM_LENGTH { get; set; }
            [Column] public int? NUMERIC_PRECISION { get; set; }
            [Column] public int? NUMERIC_SCALE { get; set; }
            [Column] public int? DATETIME_PRECISION { get; set; }
        }
    }
}
