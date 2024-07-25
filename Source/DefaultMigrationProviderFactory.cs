using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.MigrateUp.Providers;

namespace LinqToDB.MigrateUp
{
    public class DefaultMigrationProviderFactory : IMigrationProviderFactory
    {
        public IMigrationProvider CreateProvider(Migration migration)
        {
            if (migration.DataConnection.DataProvider.Name.StartsWith(ProviderName.SqlServer))
            {
                return new SqlServerProvider(migration);
            }
            else if (migration.DataConnection.DataProvider.Name.StartsWith(ProviderName.SQLite))
            {
                return new SQLiteProvider(migration);
            }
            else
            {
                return new NullProvider(migration);
            }
        }
    }
}
