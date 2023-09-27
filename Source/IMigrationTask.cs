using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp
{
    interface IMigrationTask
    {
        void Run(IMigrationProvider provider);
    }
}
