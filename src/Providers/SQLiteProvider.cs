using LinqToDB.MigrateUp.Abstractions;

namespace LinqToDB.MigrateUp.Providers;

class SQLiteProvider : MigrationProviderBase
{
    public SQLiteProvider(Migration migration, IDatabaseSchemaService schemaService, IDatabaseMutationService mutationService, IMigrationStateManager stateManager) 
        : base(migration, schemaService, mutationService, stateManager)
    {
    }


}
