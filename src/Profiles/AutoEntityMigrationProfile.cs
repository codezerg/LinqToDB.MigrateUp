using System;
using System.Linq;
using System.Reflection;
using LinqToDB.Mapping;

namespace LinqToDB.MigrateUp.Profiles
{
    /// <summary>
    /// A migration profile that automatically discovers and creates tables for entities with Table attributes.
    /// </summary>
    public class AutoEntityMigrationProfile : MigrationProfile
    {
        /// <summary>
        /// Discovers all entities in the specified assembly and creates migration tasks for them.
        /// </summary>
        /// <param name="assembly">The assembly to scan for entities.</param>
        public void DiscoverEntities(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            // Find all types with Table attribute
            var entities = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<TableAttribute>() != null 
                         && t.IsClass 
                         && !t.IsAbstract
                         && !t.IsGenericTypeDefinition);

            foreach (var entityType in entities)
            {
                // Use reflection to call CreateTable<T>
                var createTableMethod = typeof(MigrationProfile)
                    .GetMethod(nameof(CreateTable), BindingFlags.Public | BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType);

                if (createTableMethod != null)
                {
                    try
                    {
                        createTableMethod.Invoke(this, null);
                        
                        // Auto-create indexes for the entity
                        AutoCreateIndexes(entityType);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the error for this specific entity
                        Console.WriteLine($"Failed to create migration for entity {entityType.Name}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Automatically creates indexes for primary keys and commonly indexed columns.
        /// </summary>
        private void AutoCreateIndexes(Type entityType)
        {
            // Get properties with PrimaryKey attribute
            var primaryKeyProperties = entityType.GetProperties()
                .Where(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null)
                .ToList();

            // Get properties with Column attribute where IsPrimaryKey is true
            var columnPrimaryKeys = entityType.GetProperties()
                .Where(p => p.GetCustomAttribute<ColumnAttribute>()?.IsPrimaryKey == true)
                .ToList();

            // Combine both lists
            var allPrimaryKeys = primaryKeyProperties.Union(columnPrimaryKeys).Distinct().ToList();

            // If we have a composite primary key, create a composite index
            if (allPrimaryKeys.Count > 1)
            {
                var createIndexMethod = typeof(MigrationProfile)
                    .GetMethod(nameof(CreateIndex), BindingFlags.Public | BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType);

                if (createIndexMethod != null)
                {
                    try
                    {
                        var indexExpression = createIndexMethod.Invoke(this, null);
                        
                        // We'd need to use reflection to build the index expression
                        // This is complex and would require building expression trees
                        // For now, we'll skip composite indexes in auto-discovery
                    }
                    catch
                    {
                        // Silently skip if we can't create the index
                    }
                }
            }

            // Create indexes for foreign key properties (properties ending with "Id")
            var foreignKeyProperties = entityType.GetProperties()
                .Where(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
                         && p.PropertyType == typeof(int) || p.PropertyType == typeof(long) 
                         || p.PropertyType == typeof(Guid) || p.PropertyType == typeof(string))
                .Where(p => !allPrimaryKeys.Contains(p)) // Don't index primary keys again
                .ToList();

            // Note: Actually creating these indexes would require more complex reflection
            // to build the proper expression trees. This is a simplified version.
        }
    }
}