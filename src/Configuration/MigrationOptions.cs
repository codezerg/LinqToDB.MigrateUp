using LinqToDB.MigrateUp.Caching;
using LinqToDB.MigrateUp.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp.Configuration
{
    /// <summary>
    /// Configuration options for migrations.
    /// </summary>
    public class MigrationOptions
    {
        /// <summary>
        /// Gets or sets the migration cache instance. Defaults to InMemoryMigrationCache.
        /// </summary>
        public IMigrationCache Cache { get; set; } = new InMemoryMigrationCache();

        /// <summary>
        /// Gets or sets whether caching is enabled for migration execution. Defaults to true.
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to skip cached migrations. When true, migrations that have been
        /// cached as executed will be skipped. When false, all migrations will run regardless
        /// of cache state. Defaults to true.
        /// </summary>
        public bool SkipCachedMigrations { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to automatically run migrations on application startup.
        /// Used by MigrationHostedService. Defaults to false.
        /// </summary>
        public bool AutoMigrateOnStartup { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to throw an exception if migration fails during startup.
        /// If false, errors are logged but the application continues. Defaults to false.
        /// </summary>
        public bool ThrowOnMigrationFailure { get; set; } = false;
    }
}
