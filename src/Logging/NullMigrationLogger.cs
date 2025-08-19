using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp.Logging
{
    /// <summary>
    /// Represents a no-op implementation of IMigrationLogger.
    /// This logger does nothing when log methods are called.
    /// </summary>
    public class NullMigrationLogger : IMigrationLogger
    {
        public void WriteInfo(string message) { }
        public void WriteWarning(string message) { }
        public void WriteError(string message, Exception ex = null) { }
    }
}
