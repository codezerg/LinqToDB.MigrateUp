using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LinqToDB.MigrateUp.Caching
{
    /// <summary>
    /// In-memory implementation of migration cache for tracking migration execution state.
    /// </summary>
    public class InMemoryMigrationCache : IMigrationCache
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, HashSet<string>>> _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryMigrationCache"/> class.
        /// </summary>
        public InMemoryMigrationCache()
        {
            _cache = new ConcurrentDictionary<string, ConcurrentDictionary<string, HashSet<string>>>();
        }

        /// <inheritdoc/>
        public bool IsTaskExecuted(Type entityType, Type taskType, string taskHash)
        {
            if (entityType == null || taskType == null || string.IsNullOrEmpty(taskHash))
                return false;

            var entityKey = GetEntityKey(entityType);
            var taskKey = GetTaskKey(taskType);

            return _cache.TryGetValue(entityKey, out var entityCache) &&
                   entityCache.TryGetValue(taskKey, out var taskHashes) &&
                   taskHashes.Contains(taskHash);
        }

        /// <inheritdoc/>
        public void MarkTaskExecuted(Type entityType, Type taskType, string taskHash)
        {
            if (entityType == null || taskType == null || string.IsNullOrEmpty(taskHash))
                return;

            var entityKey = GetEntityKey(entityType);
            var taskKey = GetTaskKey(taskType);

            var entityCache = _cache.GetOrAdd(entityKey, _ => new ConcurrentDictionary<string, HashSet<string>>());
            var taskHashes = entityCache.GetOrAdd(taskKey, _ => new HashSet<string>());

            lock (taskHashes)
            {
                taskHashes.Add(taskHash);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Type> GetCachedEntityTypes()
        {
            return _cache.Keys.Select(key => Type.GetType(key)).Where(type => type != null);
        }

        /// <inheritdoc/>
        public void ClearEntityCache(Type entityType)
        {
            if (entityType == null)
                return;

            var entityKey = GetEntityKey(entityType);
            _cache.TryRemove(entityKey, out _);
        }

        /// <inheritdoc/>
        public void ClearAll()
        {
            _cache.Clear();
        }

        private static string GetEntityKey(Type entityType) => entityType.AssemblyQualifiedName ?? entityType.FullName;
        private static string GetTaskKey(Type taskType) => taskType.Name;
    }
}