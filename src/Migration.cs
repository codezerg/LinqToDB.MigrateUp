using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Caching;
using LinqToDB.MigrateUp.Logging;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.MigrateUp.Services;
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
        /// <summary>
        /// Gets the data connection associated with the migration.
        /// </summary>
        public DataConnection DataConnection { get; }
        public MappingSchema MappingSchema => DataConnection?.MappingSchema;
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
        public IMigrationLogger Logger { get; }

        /// <summary>
        /// Gets the collection of indexes created during migration.
        /// </summary>
        /// <remarks>Deprecated: Use StateManager.IsIndexCreated() instead.</remarks>
        public HashSet<string> IndexesCreated { get; } = new HashSet<string>();

        /// <summary>
        /// Gets the collection of tables created during migration.
        /// </summary>
        /// <remarks>Deprecated: Use StateManager.IsTableCreated() instead.</remarks>
        public HashSet<string> TablesCreated { get; } = new HashSet<string>();

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
            IMigrationLogger logger,
            MigrationOptions options = null)
        {
            DataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            StateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            Options = options ?? new MigrationOptions();
            Logger = logger ?? new NullMigrationLogger();
            MigrationProvider = (providerFactory ?? new DefaultMigrationProviderFactory()).CreateProvider(this);

            // Wire up the state manager events to maintain compatibility with legacy HashSets
            StateManager.TableCreated += (sender, tableName) => TablesCreated.Add(tableName);
            StateManager.IndexCreated += (sender, indexName) => IndexesCreated.Add(indexName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Migration"/> class with the specified data connection (legacy constructor).
        /// </summary>
        /// <param name="connection">The data connection associated with the migration.</param>
        /// <param name="options">The migration options.</param>
        /// <param name="providerFactory">The migration provider factory.</param>
        /// <param name="logger">The migration logger.</param>
        /// <remarks>This constructor is maintained for backward compatibility. Consider using the dependency injection constructor.</remarks>
        public Migration(DataConnection connection, MigrationOptions options = null, IMigrationProviderFactory providerFactory = null, IMigrationLogger logger = null)
        {
            // Set DataConnection first so it's available during provider factory creation
            DataConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            
            // Initialize services and state
            DataService = new LinqToDbDataConnectionService(connection);
            StateManager = new MigrationStateManager();
            Options = options ?? new MigrationOptions();
            Logger = logger ?? new NullMigrationLogger();
            MigrationProvider = (providerFactory ?? new DefaultMigrationProviderFactory()).CreateProvider(this);

            // Wire up the state manager events to maintain compatibility with legacy HashSets
            StateManager.TableCreated += (sender, tableName) => TablesCreated.Add(tableName);
            StateManager.IndexCreated += (sender, indexName) => IndexesCreated.Add(indexName);
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
                    Logger.WriteInfo($"Skipping cached migration task {taskType.Name} for entity {entityType.Name}");
                    return;
                }

                Logger.WriteInfo($"Executing migration task {taskType.Name} for entity {entityType.Name}");
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
