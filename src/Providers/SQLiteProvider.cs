using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.MigrateUp.Data;
using LinqToDB.MigrateUp.Execution;
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
        public SQLiteProvider(Migration migration, IDatabaseSchemaService schemaService, IDatabaseMutationService mutationService, IMigrationStateManager stateManager) 
            : base(migration, schemaService, mutationService, stateManager)
        {
        }


    }
}
