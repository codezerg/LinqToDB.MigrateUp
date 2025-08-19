using LinqToDB.MigrateUp.Logging;
using System;
using System.Collections.Generic;

namespace LinqToDB.MigrateUp.Services.Testing
{
    /// <summary>
    /// Test implementation of IMigrationLogger that captures log messages for testing purposes.
    /// </summary>
    public class TestMigrationLogger : IMigrationLogger
    {
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
            InfoMessages.Clear();
            WarningMessages.Clear();
            ErrorMessages.Clear();
        }

        /// <inheritdoc/>
        public void WriteInfo(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                InfoMessages.Add(message);
                if (WriteToConsole)
                    Console.WriteLine($"[INFO] {message}");
            }
        }

        /// <inheritdoc/>
        public void WriteWarning(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                WarningMessages.Add(message);
                if (WriteToConsole)
                    Console.WriteLine($"[WARN] {message}");
            }
        }

        /// <inheritdoc/>
        public void WriteError(string message, Exception ex = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                var errorMessage = ex != null ? $"{message}: {ex.Message}" : message;
                ErrorMessages.Add(errorMessage);
                if (WriteToConsole)
                    Console.WriteLine($"[ERROR] {errorMessage}");
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
    }
}