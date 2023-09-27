using LinqToDB.Data;
using LinqToDB.MigrateUp.Data;
using LinqToDB.SqlQuery;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp.Providers
{
    class NullProvider : MigrationProviderBase
    {
        public NullProvider(Migration migration) : base(migration)
        {
        }

        public override void Db_CreateTableColumn<Table>(string tableName, TableColumn entityColumn)
        {
        }

        public override IEnumerable<TableColumn> Db_GetTableColumns(string tableName)
        {
            return new TableColumn[0];
        }

        public override bool Db_TableExists(string tableName)
        {
            return true;
        }

        public override bool Db_TableIndexExists(string tableName, string indexName)
        {
            return false;
        }

        public override IEnumerable<TableIndexColumn> Db_GetTableIndexColumns(string tableName, string indexName)
        {
            return new TableIndexColumn[0];
        }

        public override void Db_CreateTableIndex(string tableName, string indexName, IEnumerable<TableIndexColumn> columns)
        {
        }

        public override void Db_DropTableIndex(string tableName, string indexName)
        {
        }
    }
}
