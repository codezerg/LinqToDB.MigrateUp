using System;
using LinqToDB.MigrateUp.Abstractions;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp.Abstractions
{
    public interface IMigrationProviderFactory
    {
        IMigrationProvider CreateProvider(Migration migration);
    }
}
