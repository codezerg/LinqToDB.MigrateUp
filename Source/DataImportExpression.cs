using LinqToDB.Data;
using LinqToDB.Expressions;
using LinqToDB.MigrateUp.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LinqToDB.MigrateUp
{
    /// <summary>
    /// Represents an expression to import data during migration tasks for a given table.
    /// </summary>
    /// <typeparam name="Table">The table type.</typeparam>
    public class DataImportExpression<Table> : IMigrationTask, IDataImportExpression<Table> where Table : class
    {
        public MigrationProfile Profile { get; }

        internal Func<IEnumerable<Table>> SourceFunc { get; set; }
        internal bool Option_ImportAlways { get; set; } = true;
        internal bool Option_WhenTableEmpty { get; set; }
        internal bool Option_WhenTableCreated { get; set; }

        internal Expression<Func<Table, bool>> TemplateKeyMatchExpression { get; set; }

        private ParameterExpression parameter = Expression.Parameter(typeof(Table));



        public DataImportExpression(MigrationProfile profile)
        {
            Profile = profile;
        }


        /// <summary>
        /// Sets the key selector used for matching items.
        /// </summary>
        public IDataImportExpression<Table> Key<TColumn>(Expression<Func<Table, TColumn>> keySelector)
        {
            if (!(keySelector.Body is MemberExpression memberExpression))
                throw new ArgumentException("The key selector must be a member expression.", nameof(keySelector));


            string propertyName = memberExpression.Member.Name;

            var right = keySelector.Body;
            var left = Expression.Property(parameter, propertyName);
            var body = Expression.Equal(left, right);

            if (TemplateKeyMatchExpression == null)
            {
                TemplateKeyMatchExpression = Expression.Lambda<Func<Table, bool>>(body, parameter);
            }
            else
            {
                var combinedBody = Expression.AndAlso(TemplateKeyMatchExpression.Body, body);
                TemplateKeyMatchExpression = Expression.Lambda<Func<Table, bool>>(combinedBody, parameter);
            }

            return this;
        }


        /// <summary>
        /// Sets the source function to retrieve data.
        /// </summary>
        /// <param name="source">The source function.</param>
        public IDataImportExpression<Table> Source(Func<IEnumerable<Table>> source)
        {
            SourceFunc = source;
            return this;
        }


        /// <summary>
        /// Specifies the condition to import data only if the table is empty.
        /// </summary>
        public IDataImportExpression<Table> WhenTableEmpty()
        {
            Option_WhenTableEmpty = true;
            Option_ImportAlways = false;
            return this;
        }


        /// <summary>
        /// Specifies the condition to import data only when the table is created.
        /// </summary>
        public IDataImportExpression<Table> WhenTableCreated()
        {
            Option_WhenTableCreated = true;
            Option_ImportAlways = false;
            return this;
        }


        /// <summary>
        /// Executes the migration task.
        /// </summary>
        void IMigrationTask.Run(IMigrationProvider provider)
        {
            // Ensure that the source function is defined
            if (SourceFunc == null)
                throw new InvalidOperationException("Source function is not defined.");


            // get a database connection and get the table
            var migration = provider.Migration;
            var db = migration.DataConnection;
            var table = db.GetTable<Table>();


            if (Option_ImportAlways == false)
            {
                var tableName = migration.GetEntityName<Table>();
                var tableCreated = migration.TablesCreated.Contains(tableName);
                var tableEmpty = !table.Any();

                bool shouldImport = false;

                if (tableEmpty && Option_WhenTableEmpty)
                    shouldImport = true;
                else if (tableCreated && Option_WhenTableCreated)
                    shouldImport = true;

                if (!shouldImport)
                    return;
            }


            // Retrieve data from the source
            var source = SourceFunc().ToList();
            if (!source.Any())
                return;


            // Prepare items with key matching expressions
            var sourceItems = source
                .Select(x => new
                {
                    Data = x,
                    MatchExpression = GetKeyMatchExpression(x),
                })
                .Where(x => x.MatchExpression != null)
                .Select(x => new
                {
                    x.Data,
                    x.MatchExpression,
                    MatchFunc = x.MatchExpression.Compile(),
                })
                .ToList();


            // Build a combined match expression for all source items
            var combinedExpression = BuildCombinedKeyMatchExpression(sourceItems.Select(x => x.MatchExpression));


            // Fetch existing items from the database that match the combined key expression
            var existingItems = combinedExpression == null ?
                new List<Table>() :
                table.Where(combinedExpression).ToList();


            // Determine which items from the source need to be inserted (i.e., they don't exist in the database)
            var itemsToInsert = sourceItems
                .Where(item => !existingItems.Where(item.MatchFunc).Any())
                .Select(item => item.Data)
                .ToList();


            // If there are items to insert, bulk insert them into the table
            if (itemsToInsert.Any())
            {
                db.BulkCopy(itemsToInsert);
            }
        }


        /// <summary>
        /// Builds a combined key match expression using the provided list of individual match expressions.
        /// </summary>
        /// <param name="expressions">The individual key match expressions.</param>
        /// <returns>The combined key match expression.</returns>
        private Expression<Func<Table, bool>> BuildCombinedKeyMatchExpression(IEnumerable<Expression<Func<Table, bool>>> expressions)
        {
            Expression<Func<Table, bool>> combinedExpression = null;

            foreach (var expression in expressions)
            {
                if (combinedExpression == null)
                {
                    combinedExpression = expression;
                }
                else
                {
                    combinedExpression = Expression.Lambda<Func<Table, bool>>(
                        Expression.OrElse(
                            combinedExpression.Body,
                            expression.Body),
                        combinedExpression.Parameters.Single());
                }
            }

            return combinedExpression;
        }


        /// <summary>
        /// Generates a key match expression for a given table item.
        /// </summary>
        /// <param name="item">The table item.</param>
        /// <returns>The key match expression.</returns>
        private Expression<Func<Table, bool>> GetKeyMatchExpression(Table item)
        {
            if (TemplateKeyMatchExpression == null)
                return null;

            var substituter = new QueryParameterSubstituter<Table>(parameter);
            var keyMatchExpression = substituter.SubstituteParameters(TemplateKeyMatchExpression, item);
            return keyMatchExpression;
        }

    }
}
