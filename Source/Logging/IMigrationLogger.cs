using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp.Logging
{
    public interface IMigrationLogger
    {
        void WriteInfo(string message);
        void WriteWarning(string message);
        void WriteError(string message, Exception ex = null);
    }
}
