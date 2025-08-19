using LinqToDB.MigrateUp.Logging;

namespace LinqToDB.MigrateUp.Tests.Infrastructure;

public class TestMigrationLogger : IMigrationLogger
{
    public List<string> InfoMessages { get; } = new();
    public List<string> WarningMessages { get; } = new();
    public List<(string Message, Exception? Exception)> ErrorMessages { get; } = new();

    public void WriteInfo(string message)
    {
        InfoMessages.Add(message);
    }

    public void WriteWarning(string message)
    {
        WarningMessages.Add(message);
    }

    public void WriteError(string message, Exception? ex = null)
    {
        ErrorMessages.Add((message, ex));
    }

    public void Clear()
    {
        InfoMessages.Clear();
        WarningMessages.Clear();
        ErrorMessages.Clear();
    }
}