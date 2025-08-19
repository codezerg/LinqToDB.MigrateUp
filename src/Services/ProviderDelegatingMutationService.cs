using LinqToDB.MigrateUp.Schema;
using System.Collections.Generic;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Mutation service that delegates to the provider-specific implementations for backward compatibility.
    /// </summary>
    internal class ProviderDelegatingMutationService : IDatabaseMutationService
    {
        private readonly Migration _migration;

        public ProviderDelegatingMutationService(Migration migration)
        {
            _migration = migration;
        }

        public void CreateTableColumn<TTable>(string tableName, TableColumn column)
        {
            // This will be called after the provider is created, so we can safely cast
            if (_migration.MigrationProvider is Providers.MigrationProviderBase providerBase)
            {
                // Use reflection to call the protected abstract method
                var method = providerBase.GetType().GetMethod("Db_CreateTableColumn", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    // Make the method generic for the TTable type
                    var genericMethod = method.MakeGenericMethod(typeof(TTable));
                    genericMethod.Invoke(providerBase, new object[] { tableName, column });
                }
            }
        }

        public void AlterTableColumn(string tableName, string columnName, TableColumn newColumn)
        {
            // This will be called after the provider is created, so we can safely cast
            if (_migration.MigrationProvider is Providers.MigrationProviderBase providerBase)
            {
                // Use reflection to call the protected abstract method
                var method = providerBase.GetType().GetMethod("Db_AlterTableColumn", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(providerBase, new object[] { tableName, columnName, newColumn });
                }
            }
        }

        public void CreateTableIndex(string tableName, string indexName, IEnumerable<TableIndexColumn> columns)
        {
            // This will be called after the provider is created, so we can safely cast
            if (_migration.MigrationProvider is Providers.MigrationProviderBase providerBase)
            {
                // Use reflection to call the protected abstract method
                var method = providerBase.GetType().GetMethod("Db_CreateTableIndex", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(providerBase, new object[] { tableName, indexName, columns });
                }
            }
        }

        public void DropTableIndex(string tableName, string indexName)
        {
            // This will be called after the provider is created, so we can safely cast
            if (_migration.MigrationProvider is Providers.MigrationProviderBase providerBase)
            {
                // Use reflection to call the protected abstract method
                var method = providerBase.GetType().GetMethod("Db_DropTableIndex", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(providerBase, new object[] { tableName, indexName });
                }
            }
        }
    }
}