using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Data;
using LinqToDB.MigrateUp.Providers;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.MigrateUp.Sql;
using LinqToDB.MigrateUp.Data;

namespace LinqToDB.MigrateUp
{
    public class DefaultMigrationProviderFactory : IMigrationProviderFactory
    {
        public IMigrationProvider CreateProvider(Migration migration)
        {
            // Try to detect provider type from DataService
            try
            {
                var providerName = DetectProviderType(migration);
                
                // Create provider-specific SQL query service
                ISqlQueryService queryService;
                if (providerName?.StartsWith(ProviderName.SqlServer) == true)
                {
                    queryService = new SqlServerQueryService();
                }
                else if (providerName?.StartsWith(ProviderName.SQLite) == true)
                {
                    queryService = new SQLiteQueryService();
                }
                else
                {
                    // Default to NullProvider for unknown providers
                    return new NullProvider(migration);
                }

                // Create services with proper SQL query service
                var schemaService = new DatabaseSchemaService(migration.DataService, queryService);
                var mutationService = new DatabaseMutationService(migration.DataService, queryService);
                var stateManager = migration.StateManager;
                
                if (providerName?.StartsWith(ProviderName.SqlServer) == true)
                {
                    return new SqlServerProvider(migration, schemaService, mutationService, stateManager);
                }
                else if (providerName?.StartsWith(ProviderName.SQLite) == true)
                {
                    return new SQLiteProvider(migration, schemaService, mutationService, stateManager);
                }
            }
            catch
            {
                // If provider detection fails, fallback to NullProvider
            }

            // Default to NullProvider for dependency injection scenarios or unknown providers
            return new NullProvider(migration);
        }

        private string? DetectProviderType(Migration migration)
        {
            // This is a hack to detect provider type without direct DataConnection access
            // In a proper DI setup, this would be configured explicitly
            try
            {
                // Try to get provider name through the data context
                var dataContext = migration.DataService.GetDataContext();
                if (dataContext is DataConnection dataConnection)
                {
                    return dataConnection.DataProvider?.Name;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
