using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToDB.Data;
using LinqToDB.Expressions;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.MigrateUp.Data;
using LinqToDB.MigrateUp.Execution;

namespace LinqToDB.MigrateUp.Expressions
{
    /// <summary>
    /// Represents an expression for importing data during migration tasks for a specified table.
    /// </summary>
    /// <typeparam name="TEntity">The table type.</typeparam>
    public sealed class DataImportExpression<TEntity> : IMigrationTask, IDataImportExpression<TEntity>
        where TEntity : class
    {
        private readonly IDataImportService<TEntity> _importService;
        private readonly ExpressionBuilder<TEntity> _expressionBuilder;
        private readonly DataImportConfiguration _configuration;
        
        private Func<IEnumerable<TEntity>>? _sourceFunc;
        private Expression<Func<TEntity, bool>>? _templateKeyMatchExpression;
        private readonly ParameterExpression _parameter = Expression.Parameter(typeof(TEntity));

        /// <summary>
        /// Gets the migration profile associated with this expression.
        /// </summary>
        public MigrationProfile Profile { get; }

        /// <inheritdoc/>
        public Type EntityType => typeof(TEntity);

        /// <summary>
        /// Initializes a new instance of the <see cref="DataImportExpression{TEntity}"/> class with dependency injection.
        /// </summary>
        /// <param name="profile">The migration profile associated with this expression.</param>
        /// <param name="importService">The data import service.</param>
        /// <param name="expressionBuilder">The expression builder service.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        public DataImportExpression(MigrationProfile profile, IDataImportService<TEntity> importService, ExpressionBuilder<TEntity> expressionBuilder)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
            _configuration = new DataImportConfiguration();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataImportExpression{TEntity}"/> class (legacy constructor).
        /// </summary>
        /// <param name="profile">The migration profile associated with this expression.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="profile"/> is null.</exception>
        /// <remarks>This constructor is maintained for backward compatibility.</remarks>
        public DataImportExpression(MigrationProfile profile)
            : this(profile, new DataImportService<TEntity>(), new ExpressionBuilder<TEntity>())
        {
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
            _configuration.WhenTableEmpty = true;
            _configuration.ImportAlways = false;
            return this;
        }

        /// <inheritdoc/>
        public IDataImportExpression<TEntity> WhenTableCreated()
        {
            _configuration.WhenTableCreated = true;
            _configuration.ImportAlways = false;
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
            var dataService = migration.DataService;
            var stateManager = migration.StateManager;
            var tableName = dataService.GetEntityName<TEntity>();
            var table = dataService.GetTable<TEntity>();

            bool tableHasData;
            try
            {
                tableHasData = table.Any();
            }
            catch
            {
                // If we can't query the table, assume it doesn't have data (likely doesn't exist yet)
                tableHasData = false;
            }
            
            if (!_importService.ShouldImport(_configuration, stateManager, tableName, tableHasData))
            {
                return;
            }

            var source = _sourceFunc().ToList();
            if (!source.Any())
            {
                return;
            }

            var sourceItems = PrepareSourceItems(source);
            var validExpressions = sourceItems.Select(x => x.MatchExpression).Where(x => x != null).Cast<Expression<Func<TEntity, bool>>>();
            var combinedExpression = _expressionBuilder.CombineExpressionsWithOr(validExpressions);
            var existingItems = FetchExistingItems(table, combinedExpression);
            var validMatchFunctions = sourceItems.Select(x => x.MatchFunc).Where(x => x != null).Cast<Func<TEntity, bool>>();
            var itemsToInsert = _importService.GetItemsToInsert(sourceItems.Select(x => x.Data), existingItems, validMatchFunctions);

            if (itemsToInsert.Any())
            {
                dataService.BulkCopy(itemsToInsert);
            }
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
                    MatchFunc = x.MatchExpression?.Compile()
                })
                .ToList();
        }

        private List<TEntity> FetchExistingItems(IQueryable<TEntity> table, Expression<Func<TEntity, bool>>? combinedExpression)
        {
            return combinedExpression == null ? new List<TEntity>() : table.Where(combinedExpression).ToList();
        }

        private Expression<Func<TEntity, bool>>? GetKeyMatchExpression(TEntity item)
        {
            return _expressionBuilder.BuildKeyMatchExpression(item, _templateKeyMatchExpression);
        }

        private class SourceItem
        {
            public TEntity Data { get; set; } = default!;
            public Expression<Func<TEntity, bool>>? MatchExpression { get; set; }
            public Func<TEntity, bool>? MatchFunc { get; set; }
        }
    }
}