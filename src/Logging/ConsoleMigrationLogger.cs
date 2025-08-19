using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LinqToDB.MigrateUp.Logging
{
    public class ConsoleMigrationLogger : IMigrationLogger
    {
        public void WriteInfo(string message)
        {
            Write("INFO", message, Console.WriteLine);
        }

        public void WriteWarning(string message)
        {
            Write("WARNING", message, Console.WriteLine);
        }

        public void WriteError(string message, Exception ex = null)
        {
            Write("ERROR", message, Console.Error.WriteLine);
            if (ex != null)
            {
                Write("ERROR", ex.ToString(), Console.Error.WriteLine);
            }
        }

        private void Write(string level, string message, Action<string> consoleWriter)
        {
            string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level}: {message}";
            consoleWriter(formattedMessage);
            Debug.WriteLine(formattedMessage);
        }
    }
}