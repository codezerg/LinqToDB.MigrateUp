using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace LinqToDB.MigrateUp.Tests.Testing
{
    /// <summary>
    /// Test implementation of ILogger that captures log messages for testing purposes.
    /// </summary>
    /// <typeparam name="T">The category type for the logger.</typeparam>
    public class TestLogger<T> : ILogger<T>
    {
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();

        /// <summary>
        /// Gets all log entries that have been captured.
        /// </summary>
        public IReadOnlyList<LogEntry> LogEntries => _logEntries;

        /// <summary>
        /// Gets the list of informational messages that have been logged.
        /// </summary>
        public List<string> InfoMessages { get; } = new List<string>();

        /// <summary>
        /// Gets the list of warning messages that have been logged.
        /// </summary>
        public List<string> WarningMessages { get; } = new List<string>();

        /// <summary>
        /// Gets the list of error messages that have been logged.
        /// </summary>
        public List<string> ErrorMessages { get; } = new List<string>();

        /// <summary>
        /// Gets or sets whether to also write to the console (useful for debugging tests).
        /// </summary>
        public bool WriteToConsole { get; set; } = false;

        /// <summary>
        /// Clears all captured log messages.
        /// </summary>
        public void Reset()
        {
            _logEntries.Clear();
            InfoMessages.Clear();
            WarningMessages.Clear();
            ErrorMessages.Clear();
        }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return NullDisposable.Instance;
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter == null) return;

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception == null) return;

            var logEntry = new LogEntry(logLevel, eventId, message, exception, typeof(T).Name);
            _logEntries.Add(logEntry);

            // Also populate legacy collections for backward compatibility with tests
            switch (logLevel)
            {
                case LogLevel.Information:
                    InfoMessages.Add(message);
                    if (WriteToConsole) Console.WriteLine($"[INFO] {message}");
                    break;
                case LogLevel.Warning:
                    WarningMessages.Add(message);
                    if (WriteToConsole) Console.WriteLine($"[WARN] {message}");
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    var errorMessage = exception != null ? $"{message}: {exception.Message}" : message;
                    ErrorMessages.Add(errorMessage);
                    if (WriteToConsole) Console.WriteLine($"[ERROR] {errorMessage}");
                    break;
            }
        }

        /// <summary>
        /// Gets the total number of messages logged.
        /// </summary>
        public int TotalMessageCount => InfoMessages.Count + WarningMessages.Count + ErrorMessages.Count;

        /// <summary>
        /// Gets whether any error messages have been logged.
        /// </summary>
        public bool HasErrors => ErrorMessages.Count > 0;

        /// <summary>
        /// Gets whether any warning messages have been logged.
        /// </summary>
        public bool HasWarnings => WarningMessages.Count > 0;

        private class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new NullDisposable();
            public void Dispose() { }
        }
    }

    /// <summary>
    /// Represents a log entry with all relevant information.
    /// </summary>
    public class LogEntry
    {
        public LogLevel LogLevel { get; }
        public EventId EventId { get; }
        public string Message { get; }
        public Exception? Exception { get; }
        public string Category { get; }
        public DateTime Timestamp { get; }

        public LogEntry(LogLevel logLevel, EventId eventId, string message, Exception? exception, string category)
        {
            LogLevel = logLevel;
            EventId = eventId;
            Message = message;
            Exception = exception;
            Category = category;
            Timestamp = DateTime.UtcNow;
        }
    }
}