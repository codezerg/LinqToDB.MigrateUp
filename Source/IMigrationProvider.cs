using LinqToDB.Data;
using LinqToDB.MigrateUp.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp
{
    interface IMigrationProvider
    {
        Migration Migration { get; }

        void CreateTable<Table>();
        void CreateIndex<Table>(string indexName, IEnumerable<TableIndexColumn> columns);

        void Run(MigrationProfile profile);
    }
}
