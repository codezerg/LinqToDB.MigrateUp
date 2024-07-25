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
        internal List<IMigrationTask> Tasks { get; } = new List<IMigrationTask>();

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
    }
}
