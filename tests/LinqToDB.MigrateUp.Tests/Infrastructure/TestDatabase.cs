using LinqToDB;
using LinqToDB.Data;
using LinqToDB.MigrateUp.Services;
using LinqToDB.MigrateUp.Services.Testing;
using LinqToDB.MigrateUp.Logging;
using Microsoft.Data.Sqlite;

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

    public Migration CreateMigration(MigrationOptions? options = null, IMigrationLogger? logger = null)
    {
        return new Migration(_dataConnection, options, logger: logger ?? new TestMigrationLogger());
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