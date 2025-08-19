using LinqToDB.MigrateUp.Providers;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.MigrateUp.Services;
using LinqToDB.MigrateUp.Tests.Testing;

namespace LinqToDB.MigrateUp.Tests.Providers;

/// <summary>
/// Test implementation of MigrationProviderBase for testing purposes
/// </summary>
internal class TestMigrationProvider : MigrationProviderBase
{
    private readonly MockDatabaseSchemaService _mockSchemaService;
    private readonly MockDatabaseMutationService _mockMutationService;
    private readonly MockDataConnectionService _mockDataService;

    public List<string> CreatedTables => _mockMutationService.CreatedTables.Select(x => x.TableName).ToList();
    public Dictionary<string, List<TableColumn>> CreatedColumns => 
        _mockMutationService.CreatedColumns.GroupBy(x => x.TableName)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Column).ToList());
    public List<(string TableName, string ColumnName, TableColumn NewColumn)> AlteredColumns => 
        _mockMutationService.AlteredColumns.Select(x => (x.TableName, x.ColumnName, x.NewColumn)).ToList();
    public List<(string TableName, string IndexName, IEnumerable<TableIndexColumn> Columns)> CreatedIndexes => 
        _mockMutationService.CreatedIndexes.Select(x => (x.TableName, x.IndexName, x.Columns)).ToList();
    public List<(string TableName, string IndexName)> DroppedIndexes => 
        _mockMutationService.DroppedIndexes.Select(x => (x.TableName, x.IndexName)).ToList();

    public TestMigrationProvider(Migration migration, IDatabaseSchemaService schemaService, IDatabaseMutationService mutationService, IMigrationStateManager stateManager) 
        : base(migration, schemaService, mutationService, stateManager)
    {
        // Store references to mock services for test assertions
        _mockDataService = migration.DataService as MockDataConnectionService ?? new MockDataConnectionService();
        _mockSchemaService = schemaService as MockDatabaseSchemaService ?? new MockDatabaseSchemaService();
        _mockMutationService = mutationService as MockDatabaseMutationService ?? new MockDatabaseMutationService();
    }

    public static TestMigrationProvider CreateWithMocks(Migration migration)
    {
        var mockDataService = new MockDataConnectionService();
        var mockSchemaService = new MockDatabaseSchemaService();
        var mockMutationService = new MockDatabaseMutationService();
        var mockStateManager = new MockMigrationStateManager();

        // Create a new migration with mock services
        var testMigration = new Migration(mockDataService, mockStateManager, 
            new MockProviderFactory(), new TestLogger<Migration>());

        var provider = new TestMigrationProvider(testMigration, mockSchemaService, mockMutationService, mockStateManager);
        return provider;
    }

    public void SetTableExists(string tableName, bool exists)
    {
        _mockSchemaService.SetTableExists(tableName, exists);
    }

    public void SetTableColumns(string tableName, IEnumerable<TableColumn> columns)
    {
        _mockSchemaService.SetTableColumns(tableName, columns);
    }

    public void SetIndexExists(string tableName, string indexName, bool exists)
    {
        _mockSchemaService.SetIndexExists(tableName, indexName, exists);
    }

    public void SetIndexColumns(string tableName, string indexName, IEnumerable<TableIndexColumn> columns)
    {
        _mockSchemaService.SetIndexColumns(tableName, indexName, columns);
    }

}