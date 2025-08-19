using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToDB.MigrateUp
{
    /// <summary>
    /// Provides an abstract base for defining migration profiles
    /// </summary>
    public abstract class MigrationProfile
    {
        /// <summary>
        /// Gets the list of migration tasks associated with this migration profile.
        /// </summary>
        public List<IMigrationTask> Tasks { get; } = new List<IMigrationTask>();

        /// <summary>
        /// Creates a new table creation expression for a specified table type.
        /// </summary>
        /// <typeparam name="TEntity">The type representing the table to be created.</typeparam>
        /// <returns>An instance of <see cref="ICreateTableExpression{TEntity}"/> for the specified table type.</returns>
        public ICreateTableExpression<TEntity> CreateTable<TEntity>() where TEntity : class
        {
            var expr = new CreateTableExpression<TEntity>(this);
            Tasks.Add(expr);
            return expr;
        }

        /// <summary>
        /// Creates a new index creation expression for a specified table type.
        /// </summary>
        /// <typeparam name="TEntity">The type representing the table for which the index will be created.</typeparam>
        /// <returns>An instance of <see cref="ICreateIndexExpression{TEntity}"/> for the specified table type.</returns>
        public ICreateIndexExpression<TEntity> CreateIndex<TEntity>() where TEntity : class
        {
            var expr = new CreateIndexExpression<TEntity>(this);
            Tasks.Add(expr);
            return expr;
        }

        /// <summary>
        /// Creates a new data import expression for a specified table type.
        /// </summary>
        /// <typeparam name="TEntity">The type representing the table into which data will be imported.</typeparam>
        /// <returns>An instance of <see cref="IDataImportExpression{TEntity}"/> for the specified table type.</returns>
        public IDataImportExpression<TEntity> ImportData<TEntity>() where TEntity : class
        {
            var expr = new DataImportExpression<TEntity>(this);
            Tasks.Add(expr);
            return expr;
        }

        /// <summary>
        /// Gets all migration tasks for a specific entity type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to filter tasks for.</typeparam>
        /// <returns>Collection of migration tasks for the specified entity type.</returns>
        public IEnumerable<IMigrationTask> GetTasksForEntity<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);
            return Tasks.Where(task => task.EntityType == entityType);
        }

        /// <summary>
        /// Gets all migration tasks for a specific entity type.
        /// </summary>
        /// <param name="entityType">The entity type to filter tasks for.</param>
        /// <returns>Collection of migration tasks for the specified entity type.</returns>
        public IEnumerable<IMigrationTask> GetTasksForEntity(Type entityType)
        {
            return Tasks.Where(task => task.EntityType == entityType);
        }

        /// <summary>
        /// Gets all unique entity types that have migration tasks defined in this profile.
        /// </summary>
        /// <returns>Collection of entity types.</returns>
        public IEnumerable<Type> GetEntityTypes()
        {
            return Tasks.Select(task => task.EntityType).Distinct();
        }
    }
}
