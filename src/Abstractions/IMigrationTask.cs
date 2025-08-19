using System;
using LinqToDB.MigrateUp.Abstractions;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp.Abstractions;

/// <summary>
/// Defines the contract for migration tasks.
/// </summary>
public interface IMigrationTask
{
    /// <summary>
    /// Gets the entity type this task applies to.
    /// </summary>
    Type EntityType { get; }

    /// <summary>
    /// Runs the migration task.
    /// </summary>
    /// <param name="provider">The migration provider to use for executing the task.</param>
    void Run(IMigrationProvider provider);
}
