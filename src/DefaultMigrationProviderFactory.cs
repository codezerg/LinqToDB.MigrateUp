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
            // If we have a DataConnection, use provider-specific implementations
            if (migration.DataConnection != null)
            {
                if (migration.DataConnection.DataProvider.Name.StartsWith(ProviderName.SqlServer))
                {
                    return new SqlServerProvider(migration);
                }
                else if (migration.DataConnection.DataProvider.Name.StartsWith(ProviderName.SQLite))
                {
                    return new SQLiteProvider(migration);
                }
            }

            // Default to NullProvider for dependency injection scenarios or unknown providers
            return new NullProvider(migration);
        }
    }
}
