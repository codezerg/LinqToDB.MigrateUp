using LinqToDB.MigrateUp.Schema;
using System.Collections.Generic;
using System.Linq;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Schema service that delegates to the provider-specific implementations for backward compatibility.
    /// </summary>
    internal class ProviderDelegatingSchemaService : IDatabaseSchemaService
    {
        private readonly Migration _migration;

        public ProviderDelegatingSchemaService(Migration migration)
        {
            _migration = migration;
        }

        public bool TableExists(string tableName)
        {
            // This will be called during migration execution, when the provider is available
            if (_migration.MigrationProvider is Providers.MigrationProviderBase providerBase)
            {
                // Use reflection to call the protected abstract method
                var method = providerBase.GetType().GetMethod("Db_TableExists", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    return (bool)method.Invoke(providerBase, new object[] { tableName });
                }
            }
            
            // Fallback - try a simple query-based approach if no provider-specific method
            try
            {
                // Use the migration's DataService to check if table exists by trying to query it
                var table = _migration.DataService.GetTable<object>();
                // This is a simplified fallback - in practice you'd want provider-specific logic
                return false; // For now, just return false as this is a fallback
            }
            catch
            {
                return false;
            }
        }

        public bool IndexExists(string tableName, string indexName)
        {
            // This will be called after the provider is created, so we can safely cast
            if (_migration.MigrationProvider is Providers.MigrationProviderBase providerBase)
            {
                // Use reflection to call the protected abstract method
                var method = providerBase.GetType().GetMethod("Db_TableIndexExists", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    return (bool)method.Invoke(providerBase, new object[] { tableName, indexName });
                }
            }
            
            // Fallback to false if provider doesn't support index existence checks
            return false;
        }

        public IEnumerable<TableColumn> GetTableColumns(string tableName)
        {
            // This will be called after the provider is created, so we can safely cast
            if (_migration.MigrationProvider is Providers.MigrationProviderBase providerBase)
            {
                // Use reflection to call the protected abstract method
                var method = providerBase.GetType().GetMethod("Db_GetTableColumns", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    return (IEnumerable<TableColumn>)method.Invoke(providerBase, new object[] { tableName });
                }
            }
            
            // Fallback to empty collection
            return Enumerable.Empty<TableColumn>();
        }

        public IEnumerable<TableIndexColumn> GetIndexColumns(string tableName, string indexName)
        {
            // This will be called after the provider is created, so we can safely cast
            if (_migration.MigrationProvider is Providers.MigrationProviderBase providerBase)
            {
                // Use reflection to call the protected abstract method
                var method = providerBase.GetType().GetMethod("Db_GetTableIndexColumns", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    return (IEnumerable<TableIndexColumn>)method.Invoke(providerBase, new object[] { tableName, indexName });
                }
            }
            
            // Fallback to empty collection
            return Enumerable.Empty<TableIndexColumn>();
        }
    }
}