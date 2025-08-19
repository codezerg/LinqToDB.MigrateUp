using LinqToDB;
using LinqToDB.Data;
using LinqToDB.MigrateUp.Services;
using LinqToDB.MigrateUp.Tests.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace LinqToDB.MigrateUp.Tests.Infrastructure;

public class TestDatabase : IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _connection;
    private DataConnection _dataConnection;

    public TestDatabase()
    {
        _connectionString = "Data Source=:memory:";
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();
        _dataConnection = new DataConnection(ProviderName.SQLite, _connectionString);
    }

    public DataConnection CreateConnection()
    {
        // Return the persistent connection to avoid disposal issues
        return _dataConnection;
    }

    public Migration CreateMigration(MigrationOptions? options = null, ILogger<Migration>? logger = null)
    {
        var dataService = new LinqToDbDataConnectionService(_dataConnection);
        var stateManager = new MigrationStateManager();
        var providerFactory = new DefaultMigrationProviderFactory();
        return new Migration(dataService, stateManager, providerFactory, logger ?? new TestLogger<Migration>(), options);
    }

    public MigrationTestBuilder CreateMigrationBuilder()
    {
        var dataService = new LinqToDbDataConnectionService(_dataConnection);
        return new MigrationTestBuilder()
            .WithDataService(dataService);
    }

    public void Dispose()
    {
        _dataConnection?.Dispose();
        _connection?.Dispose();
    }
}