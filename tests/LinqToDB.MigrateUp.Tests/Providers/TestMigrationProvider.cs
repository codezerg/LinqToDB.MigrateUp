using LinqToDB.MigrateUp.Providers;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.MigrateUp.Services;
using LinqToDB.MigrateUp.Services.Testing;

namespace LinqToDB.MigrateUp.Tests.Providers;

/// <summary>
/// Test implementation of MigrationProviderBase for testing purposes
/// </summary>
internal class TestMigrationProvider : MigrationProviderBase
{
    private readonly MockDatabaseSchemaService _mockSchemaService;
    private readonly MockDatabaseMutationService _mockMutationService;
    private readonly MockDataConnectionService _mockDataService;

    public List<string> CreatedTables => _mockDataService.CreatedTables;
    public Dictionary<string, List<TableColumn>> CreatedColumns { get; } = new();
    public List<(string TableName, string ColumnName, TableColumn NewColumn)> AlteredColumns => 
        _mockMutationService.AlteredColumns.Select(x => (x.TableName, x.ColumnName, x.NewColumn)).ToList();
    public List<(string TableName, string IndexName, IEnumerable<TableIndexColumn> Columns)> CreatedIndexes => 
        _mockMutationService.CreatedIndexes.Select(x => (x.TableName, x.IndexName, x.Columns)).ToList();
    public List<(string TableName, string IndexName)> DroppedIndexes => 
        _mockMutationService.DroppedIndexes.Select(x => (x.TableName, x.IndexName)).ToList();

    public TestMigrationProvider(Migration migration) : base(migration)
    {
        // Initialize mock services
        _mockDataService = new MockDataConnectionService();
        _mockSchemaService = new MockDatabaseSchemaService();
        _mockMutationService = new MockDatabaseMutationService();
    }

    public static TestMigrationProvider CreateWithMocks(Migration migration)
    {
        var mockDataService = new MockDataConnectionService();
        var mockSchemaService = new MockDatabaseSchemaService();
        var mockMutationService = new MockDatabaseMutationService();
        var mockStateManager = new MockMigrationStateManager();

        // Create a new migration with mock services
        var testMigration = new Migration(mockDataService, mockStateManager, 
            new MockProviderFactory(), new TestMigrationLogger());

        var provider = new TestMigrationProvider(testMigration);
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

    // Override the abstract methods to use our mock services
    protected override bool Db_TableExists(string tableName)
    {
        return _mockSchemaService.TableExists(tableName);
    }

    protected override IEnumerable<TableColumn> Db_GetTableColumns(string tableName)
    {
        return _mockSchemaService.GetTableColumns(tableName);
    }

    protected override void Db_CreateTableColumn<TTable>(string tableName, TableColumn column)
    {
        // Track created columns for test assertions
        if (!CreatedColumns.ContainsKey(tableName))
            CreatedColumns[tableName] = new List<TableColumn>();
        
        CreatedColumns[tableName].Add(column);
        
        // Delegate to mock mutation service
        _mockMutationService.CreateTableColumn<TTable>(tableName, column);
    }

    protected override void Db_AlterTableColumn(string tableName, string columnName, TableColumn newColumn)
    {
        _mockMutationService.AlterTableColumn(tableName, columnName, newColumn);
    }

    protected override bool Db_TableIndexExists(string tableName, string indexName)
    {
        return _mockSchemaService.IndexExists(tableName, indexName);
    }

    protected override IEnumerable<TableIndexColumn> Db_GetTableIndexColumns(string tableName, string indexName)
    {
        return _mockSchemaService.GetIndexColumns(tableName, indexName);
    }

    protected override void Db_CreateTableIndex(string tableName, string indexName, IEnumerable<TableIndexColumn> columns)
    {
        _mockMutationService.CreateTableIndex(tableName, indexName, columns);
    }

    protected override void Db_DropTableIndex(string tableName, string indexName)
    {
        _mockMutationService.DropTableIndex(tableName, indexName);
    }
}