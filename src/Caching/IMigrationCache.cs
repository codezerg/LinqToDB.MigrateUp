using System;
using System.Collections.Generic;

namespace LinqToDB.MigrateUp.Caching
{
    /// <summary>
    /// Defines the contract for caching migration execution state.
    /// </summary>
    public interface IMigrationCache
    {
        /// <summary>
        /// Checks if a migration task has been executed for the specified entity type.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="taskType">The migration task type.</param>
        /// <param name="taskHash">The hash of the task configuration.</param>
        /// <returns>True if the task has been executed and cached, false otherwise.</returns>
        bool IsTaskExecuted(Type entityType, Type taskType, string taskHash);

        /// <summary>
        /// Marks a migration task as executed for the specified entity type.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="taskType">The migration task type.</param>
        /// <param name="taskHash">The hash of the task configuration.</param>
        void MarkTaskExecuted(Type entityType, Type taskType, string taskHash);

        /// <summary>
        /// Gets all cached entity types that have had migrations executed.
        /// </summary>
        /// <returns>Collection of entity types.</returns>
        IEnumerable<Type> GetCachedEntityTypes();

        /// <summary>
        /// Clears the cache for a specific entity type.
        /// </summary>
        /// <param name="entityType">The entity type to clear cache for.</param>
        void ClearEntityCache(Type entityType);

        /// <summary>
        /// Clears all cached migration state.
        /// </summary>
        void ClearAll();
    }
}