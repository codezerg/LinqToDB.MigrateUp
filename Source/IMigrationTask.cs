using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp
{
    /// <summary>
    /// Defines the contract for migration tasks.
    /// </summary>
    interface IMigrationTask
    {
        /// <summary>
        /// Runs the migration task.
        /// </summary>
        /// <param name="provider">The migration provider to use for executing the task.</param>
        void Run(IMigrationProvider provider);
    }
}
