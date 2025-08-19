using LinqToDB.MigrateUp.Caching;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp
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
    }
}
