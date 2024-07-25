using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Logging;
using LinqToDB.MigrateUp.Schema;
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
        public MappingSchema MappingSchema => DataConnection.MappingSchema;
        internal IMigrationProvider MigrationProvider { get; }


        public MigrationOptions Options { get; }
        public IMigrationLogger Logger { get; }


        internal HashSet<string> IndexesCreated { get; } = new HashSet<string>();
        internal HashSet<string> TablesCreated { get; } = new HashSet<string>();


        /// <summary>
        /// Initializes a new instance of the <see cref="Migration"/> class with the specified data connection.
        /// </summary>
        /// <param name="connection">The data connection associated with the migration.</param>
        public Migration(DataConnection connection, MigrationOptions options = null, IMigrationProviderFactory providerFactory = null, IMigrationLogger logger = null)
        {
            DataConnection = connection;
            Options = options ?? new MigrationOptions();
            MigrationProvider = (providerFactory ?? new DefaultMigrationProviderFactory()).CreateProvider(this);
            Logger = logger ?? new NullMigrationLogger();
        }


        internal string GetEntityName<TEntity>()
        {
            return MappingSchema.GetEntityDescriptor(typeof(TEntity)).Name.Name;
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
                    task.Run(MigrationProvider);
                }
            }
        }
    }
}
