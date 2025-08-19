using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Caching;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.MigrateUp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToDB.MigrateUp
{
    /// <summary>
    /// Represents a migration process, facilitating the creation, alteration, or removal of database elements.
    /// </summary>
    public class Migration
    {
        public MappingSchema MappingSchema => DataService.GetMappingSchema();
        internal IMigrationProvider MigrationProvider { get; }

        /// <summary>
        /// Gets the data connection service for abstracted database operations.
        /// </summary>
        public IDataConnectionService DataService { get; }

        /// <summary>
        /// Gets the migration state manager for tracking operations.
        /// </summary>
        public IMigrationStateManager StateManager { get; }

        public MigrationOptions Options { get; }
        public ILogger<Migration> Logger { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Migration"/> class with dependency injection support.
        /// </summary>
        /// <param name="dataService">The data connection service.</param>
        /// <param name="stateManager">The migration state manager.</param>
        /// <param name="providerFactory">The migration provider factory.</param>
        /// <param name="logger">The migration logger.</param>
        /// <param name="options">The migration options.</param>
        public Migration(
            IDataConnectionService dataService,
            IMigrationStateManager stateManager,
            IMigrationProviderFactory providerFactory,
            ILogger<Migration> logger,
            MigrationOptions? options = null)
        {
            DataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            StateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            Options = options ?? new MigrationOptions();
            Logger = logger ?? NullLogger<Migration>.Instance;
            MigrationProvider = (providerFactory ?? new DefaultMigrationProviderFactory()).CreateProvider(this);

        }



        internal string GetEntityName<TEntity>() where TEntity : class
        {
            return DataService.GetEntityName<TEntity>();
        }


        /// <summary>
        /// Executes the migration tasks defined in the given migration configuration.
        /// </summary>
        /// <param name="configuration">The migration configuration containing profiles and tasks to execute.</param>
        public void Run(MigrationConfiguration configuration)
        {
            foreach (var profile in configuration.Profiles)
            {
                foreach (var task in profile.Tasks)
                {
                    RunTask(task);
                }
            }
        }

        /// <summary>
        /// Executes migration tasks for a specific entity type only.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to run migrations for.</typeparam>
        /// <param name="configuration">The migration configuration containing profiles and tasks to execute.</param>
        public void RunForEntity<TEntity>(MigrationConfiguration configuration) where TEntity : class
        {
            var entityType = typeof(TEntity);
            
            foreach (var profile in configuration.Profiles)
            {
                var entityTasks = profile.Tasks.Where(task => task.EntityType == entityType).ToList();
                foreach (var task in entityTasks)
                {
                    RunTask(task);
                }
            }
        }

        /// <summary>
        /// Executes a single migration task with caching support.
        /// </summary>
        /// <param name="task">The migration task to execute.</param>
        private void RunTask(IMigrationTask task)
        {
            if (Options.EnableCaching && Options.SkipCachedMigrations)
            {
                var taskHash = MigrationTaskHasher.GenerateHash(task);
                var taskType = task.GetType();
                var entityType = task.EntityType;

                if (Options.Cache.IsTaskExecuted(entityType, taskType, taskHash))
                {
                    Logger.LogInformation("Skipping cached migration task {TaskType} for entity {EntityType}", taskType.Name, entityType.Name);
                    return;
                }

                Logger.LogInformation("Executing migration task {TaskType} for entity {EntityType}", taskType.Name, entityType.Name);
                task.Run(MigrationProvider);

                Options.Cache.MarkTaskExecuted(entityType, taskType, taskHash);
            }
            else
            {
                task.Run(MigrationProvider);
            }
        }
    }
}
