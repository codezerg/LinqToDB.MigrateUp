using LinqToDB.Mapping;
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
        // List of migration tasks associated with this migration profile.
        internal List<IMigrationTask> Tasks { get; } = new List<IMigrationTask>();

        /// <summary>
        /// Creates a new table creation expression for a specified table type and adds it to the migration tasks list.
        /// </summary>
        /// <typeparam name="Table">The type representing the table to be created.</typeparam>
        /// <returns>An instance of <see cref="ICreateTableExpression{Table}"/> for the specified table type.</returns>
        public ICreateTableExpression<Table> CreateTable<Table>() where Table : class
        {
            var expr = new CreateTableExpression<Table>(this);
            Tasks.Add(expr);
            return expr;
        }

        /// <summary>
        /// Creates a new index creation expression for a specified table type and adds it to the migration tasks list.
        /// </summary>
        /// <typeparam name="Table">The type representing the table for which the index will be created.</typeparam>
        /// <returns>An instance of <see cref="ICreateIndexExpression{Table}"/> for the specified table type.</returns>
        public ICreateIndexExpression<Table> CreateIndex<Table>() where Table : class
        {
            var expr = new CreateIndexExpression<Table>(this);
            Tasks.Add(expr);
            return expr;
        }

        /// <summary>
        /// Creates a new data import expression for a specified table type and adds it to the migration tasks list.
        /// </summary>
        /// <typeparam name="Table">The type representing the table into which data will be imported.</typeparam>
        /// <returns>An instance of <see cref="IDataImportExpression{Table}"/> for the specified table type.</returns>
        public IDataImportExpression<Table> ImportData<Table>() where Table : class
        {
            var expr = new DataImportExpression<Table>(this);
            Tasks.Add(expr);
            return expr;
        }
    }
}
