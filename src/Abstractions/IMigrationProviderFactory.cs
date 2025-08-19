using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp
{
    public interface IMigrationProviderFactory
    {
        IMigrationProvider CreateProvider(Migration migration);
    }
}
