using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp
{
    public class MigrationException : Exception
    {
        public MigrationException(string message, Exception? innerException = null) : base(message, innerException) { }
    }
}
