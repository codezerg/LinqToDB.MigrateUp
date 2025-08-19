using Microsoft.Extensions.Logging;
using System;
using LinqToDB.MigrateUp;
using LinqToDB.MigrateUp.Services;

namespace LinqToDB.MigrateUp.Tests.Testing
{
    /// <summary>
    /// Builder pattern for creating Migration instances with configurable dependencies for testing.
    /// </summary>
    public class MigrationTestBuilder
    {
        private IDataConnectionService _dataService;
        private IMigrationStateManager _stateManager;
        private IMigrationProviderFactory _providerFactory;
        private ILogger<Migration> _logger;
        private MigrationOptions _options;

        /// <summary>
        /// Initializes a new instance of the MigrationTestBuilder with default mock services.
        /// </summary>
        public MigrationTestBuilder()
        {
            _dataService = new MockDataConnectionService();
            _stateManager = new MockMigrationStateManager();
            _providerFactory = new MockProviderFactory();
            _logger = new TestLogger<Migration>();
            _options = new MigrationOptions();
        }

        /// <summary>
        /// Sets the data connection service to use.
        /// </summary>
        /// <param name="service">The data connection service.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public MigrationTestBuilder WithDataService(IDataConnectionService service)
        {
            _dataService = service ?? throw new ArgumentNullException(nameof(service));
            return this;
        }

        /// <summary>
        /// Sets the migration state manager to use.
        /// </summary>
        /// <param name="stateManager">The migration state manager.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public MigrationTestBuilder WithStateManager(IMigrationStateManager stateManager)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            return this;
        }

        /// <summary>
        /// Sets the migration provider factory to use.
        /// </summary>
        /// <param name="providerFactory">The migration provider factory.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public MigrationTestBuilder WithProviderFactory(IMigrationProviderFactory providerFactory)
        {
            _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            return this;
        }

        /// <summary>
        /// Sets the migration logger to use.
        /// </summary>
        /// <param name="logger">The migration logger.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public MigrationTestBuilder WithLogger(ILogger<Migration> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            return this;
        }

        /// <summary>
        /// Sets the migration options to use.
        /// </summary>
        /// <param name="options">The migration options.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public MigrationTestBuilder WithOptions(MigrationOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            return this;
        }

        /// <summary>
        /// Configures the builder to use a mock data service with the specified table data.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="data">The mock data for the table.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public MigrationTestBuilder WithMockTableData<T>(params T[] data) where T : class
        {
            var mockDataService = _dataService as MockDataConnectionService ?? new MockDataConnectionService();
            mockDataService.SetupTableData(data);
            _dataService = mockDataService;
            return this;
        }

        /// <summary>
        /// Configures the builder to use a mock state manager with pre-existing created tables.
        /// </summary>
        /// <param name="createdTables">The names of tables to mark as already created.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public MigrationTestBuilder WithCreatedTables(params string[] createdTables)
        {
            var mockStateManager = _stateManager as MockMigrationStateManager ?? new MockMigrationStateManager();
            foreach (var tableName in createdTables)
            {
                mockStateManager.MarkTableCreated(tableName);
            }
            _stateManager = mockStateManager;
            return this;
        }

        /// <summary>
        /// Configures the builder to use a mock state manager with pre-existing created indexes.
        /// </summary>
        /// <param name="createdIndexes">The names of indexes to mark as already created.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public MigrationTestBuilder WithCreatedIndexes(params string[] createdIndexes)
        {
            var mockStateManager = _stateManager as MockMigrationStateManager ?? new MockMigrationStateManager();
            foreach (var indexName in createdIndexes)
            {
                mockStateManager.MarkIndexCreated(indexName);
            }
            _stateManager = mockStateManager;
            return this;
        }

        /// <summary>
        /// Builds the Migration instance with the configured dependencies.
        /// </summary>
        /// <returns>A new Migration instance configured for testing.</returns>
        public Migration Build()
        {
            return new Migration(_dataService, _stateManager, _providerFactory, _logger, _options);
        }

        /// <summary>
        /// Gets the configured data service as a MockDataConnectionService.
        /// </summary>
        /// <returns>The mock data connection service if configured, null otherwise.</returns>
        public MockDataConnectionService? GetMockDataService()
        {
            return _dataService as MockDataConnectionService;
        }

        /// <summary>
        /// Gets the configured state manager as a MockMigrationStateManager.
        /// </summary>
        /// <returns>The mock migration state manager if configured, null otherwise.</returns>
        public MockMigrationStateManager? GetMockStateManager()
        {
            return _stateManager as MockMigrationStateManager;
        }

        /// <summary>
        /// Gets the configured logger as a TestLogger.
        /// </summary>
        /// <returns>The test logger if configured, null otherwise.</returns>
        public TestLogger<Migration>? GetTestLogger()
        {
            return _logger as TestLogger<Migration>;
        }
    }
}