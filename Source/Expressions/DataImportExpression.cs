using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToDB.Data;
using LinqToDB.Expressions;

namespace LinqToDB.MigrateUp.Expressions
{
    /// <summary>
    /// Represents an expression for importing data during migration tasks for a specified table.
    /// </summary>
    /// <typeparam name="TEntity">The table type.</typeparam>
    internal sealed class DataImportExpression<TEntity> : IMigrationTask, IDataImportExpression<TEntity>
        where TEntity : class
    {
        private Func<IEnumerable<TEntity>> _sourceFunc;
        private bool _importAlways = true;
        private bool _whenTableEmpty;
        private bool _whenTableCreated;
        private Expression<Func<TEntity, bool>> _templateKeyMatchExpression;
        private readonly ParameterExpression _parameter = Expression.Parameter(typeof(TEntity));

        /// <summary>
        /// Gets the migration profile associated with this expression.
        /// </summary>
        public MigrationProfile Profile { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataImportExpression{TEntity}"/> class.
        /// </summary>
        /// <param name="profile">The migration profile associated with this expression.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="profile"/> is null.</exception>
        public DataImportExpression(MigrationProfile profile)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }

        /// <inheritdoc/>
        public IDataImportExpression<TEntity> Key<TColumn>(Expression<Func<TEntity, TColumn>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (!(keySelector.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException("The key selector must be a member expression.", nameof(keySelector));
            }

            string propertyName = memberExpression.Member.Name;
            var left = Expression.Property(_parameter, propertyName);
            var body = Expression.Equal(left, keySelector.Body);

            _templateKeyMatchExpression = _templateKeyMatchExpression == null
                ? Expression.Lambda<Func<TEntity, bool>>(body, _parameter)
                : Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(_templateKeyMatchExpression.Body, body), _parameter);

            return this;
        }

        /// <inheritdoc/>
        public IDataImportExpression<TEntity> Source(Func<IEnumerable<TEntity>> source)
        {
            _sourceFunc = source ?? throw new ArgumentNullException(nameof(source));
            return this;
        }

        /// <inheritdoc/>
        public IDataImportExpression<TEntity> WhenTableEmpty()
        {
            _whenTableEmpty = true;
            _importAlways = false;
            return this;
        }

        /// <inheritdoc/>
        public IDataImportExpression<TEntity> WhenTableCreated()
        {
            _whenTableCreated = true;
            _importAlways = false;
            return this;
        }

        /// <inheritdoc/>
        void IMigrationTask.Run(IMigrationProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (_sourceFunc == null)
            {
                throw new InvalidOperationException("Source function is not defined.");
            }

            var migration = provider.Migration;
            var db = migration.DataConnection;
            var table = db.GetTable<TEntity>();

            if (!ShouldImport(migration, table))
            {
                return;
            }

            var source = _sourceFunc().ToList();
            if (!source.Any())
            {
                return;
            }

            var sourceItems = PrepareSourceItems(source);
            var combinedExpression = BuildCombinedKeyMatchExpression(sourceItems.Select(x => x.MatchExpression));
            var existingItems = FetchExistingItems(table, combinedExpression);
            var itemsToInsert = GetItemsToInsert(sourceItems, existingItems);

            if (itemsToInsert.Any())
            {
                db.BulkCopy(itemsToInsert);
            }
        }

        private bool ShouldImport(Migration migration, ITable<TEntity> table)
        {
            if (_importAlways)
            {
                return true;
            }

            var tableName = migration.GetEntityName<TEntity>();
            var tableCreated = migration.TablesCreated.Contains(tableName);
            var tableEmpty = !table.Any();

            return (tableEmpty && _whenTableEmpty) || (tableCreated && _whenTableCreated);
        }

        private List<SourceItem> PrepareSourceItems(List<TEntity> source)
        {
            return source
                .Select(x => new SourceItem
                {
                    Data = x,
                    MatchExpression = GetKeyMatchExpression(x)
                })
                .Where(x => x.MatchExpression != null)
                .Select(x => new SourceItem
                {
                    Data = x.Data,
                    MatchExpression = x.MatchExpression,
                    MatchFunc = x.MatchExpression.Compile()
                })
                .ToList();
        }

        private Expression<Func<TEntity, bool>> BuildCombinedKeyMatchExpression(IEnumerable<Expression<Func<TEntity, bool>>> expressions)
        {
            return expressions.Aggregate((Expression<Func<TEntity, bool>>)null, (current, expression) =>
            {
                if (current == null)
                {
                    return expression;
                }

                var body = Expression.OrElse(current.Body, expression.Body);
                return Expression.Lambda<Func<TEntity, bool>>(body, current.Parameters.Single());
            });
        }

        private List<TEntity> FetchExistingItems(ITable<TEntity> table, Expression<Func<TEntity, bool>> combinedExpression)
        {
            return combinedExpression == null ? new List<TEntity>() : table.Where(combinedExpression).ToList();
        }

        private List<TEntity> GetItemsToInsert(List<SourceItem> sourceItems, List<TEntity> existingItems)
        {
            return sourceItems
                .Where(item => !existingItems.Any(item.MatchFunc))
                .Select(item => item.Data)
                .ToList();
        }

        private Expression<Func<TEntity, bool>> GetKeyMatchExpression(TEntity item)
        {
            if (_templateKeyMatchExpression == null)
            {
                return null;
            }

            var substituter = new QueryParameterSubstituter<TEntity>(_parameter);
            return substituter.SubstituteParameters(_templateKeyMatchExpression, item);
        }

        private class SourceItem
        {
            public TEntity Data { get; set; }
            public Expression<Func<TEntity, bool>> MatchExpression { get; set; }
            public Func<TEntity, bool> MatchFunc { get; set; }
        }
    }
}