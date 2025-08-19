using LinqToDB.MigrateUp;
using LinqToDB.MigrateUp.Services;

namespace LinqToDB.MigrateUp.Tests.Testing
{
    /// <summary>
    /// Mock implementation of IMigrationProviderFactory for testing purposes.
    /// </summary>
    public class MockProviderFactory : IMigrationProviderFactory
    {
        /// <summary>
        /// Gets or sets the provider instance to return from CreateProvider.
        /// </summary>
        public IMigrationProvider? Provider { get; set; }

        /// <summary>
        /// Gets or sets whether to create a mock provider automatically if none is set.
        /// </summary>
        public bool CreateMockProvider { get; set; } = true;

        /// <inheritdoc/>
        public IMigrationProvider CreateProvider(Migration migration)
        {
            if (Provider != null)
                return Provider;

            if (CreateMockProvider)
                return new MockMigrationProvider(migration);

            return new DefaultMigrationProviderFactory().CreateProvider(migration);
        }
    }

    /// <summary>
    /// Mock implementation of IMigrationProvider for testing purposes.
    /// </summary>
    public class MockMigrationProvider : IMigrationProvider
    {
        /// <summary>
        /// Gets the migration associated with this provider.
        /// </summary>
        public Migration Migration { get; }

        /// <summary>
        /// Gets or sets whether operations should throw exceptions (for testing error scenarios).
        /// </summary>
        public bool ThrowOnOperations { get; set; } = false;

        /// <summary>
        /// Gets or sets the exception message to throw when ThrowOnOperations is true.
        /// </summary>
        public string ExceptionMessage { get; set; } = "Mock migration operation failed";

        /// <summary>
        /// Initializes a new instance of the MockMigrationProvider.
        /// </summary>
        /// <param name="migration">The migration instance.</param>
        public MockMigrationProvider(Migration migration)
        {
            Migration = migration ?? throw new System.ArgumentNullException(nameof(migration));
        }

        /// <inheritdoc/>
        public void UpdateTableSchema<TEntity>() where TEntity : class
        {
            if (ThrowOnOperations)
                throw new System.InvalidOperationException(ExceptionMessage);

            // Mock implementation - no actual database operations
            var tableName = Migration.DataService.GetEntityName<TEntity>();
            Migration.StateManager.MarkTableCreated(tableName);
        }

        /// <inheritdoc/>
        public void EnsureIndex<TEntity>(string indexName, System.Collections.Generic.IEnumerable<Schema.TableIndexColumn> columns) where TEntity : class
        {
            if (ThrowOnOperations)
                throw new System.InvalidOperationException(ExceptionMessage);

            // Mock implementation - no actual database operations
            Migration.StateManager.MarkIndexCreated(indexName);
        }
    }
}