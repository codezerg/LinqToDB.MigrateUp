using LinqToDB.MigrateUp.Abstractions;

namespace LinqToDB.MigrateUp.Providers;

class SqlServerProvider : MigrationProviderBase
{
    public SqlServerProvider(Migration migration, IDatabaseSchemaService schemaService, IDatabaseMutationService mutationService, IMigrationStateManager stateManager) 
        : base(migration, schemaService, mutationService, stateManager)
    {
    }


}
