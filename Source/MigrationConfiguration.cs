using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToDB.MigrateUp
{
    /// <summary>
    /// Represents a configuration for database migrations.
    /// </summary>
    public class MigrationConfiguration
    {
        /// <summary>
        /// Gets the list of migration profiles associated with this configuration.
        /// </summary>
        public List<MigrationProfile> Profiles { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationConfiguration"/> class using a configuration expression.
        /// </summary>
        /// <param name="configure">A delegate to configure the migration.</param>
        public MigrationConfiguration(Action<MigrationConfigurationExpression> configure) : this(Build(configure)) { }
        static MigrationConfigurationExpression Build(Action<MigrationConfigurationExpression> configure)
        {
            var expr = new MigrationConfigurationExpression();
            configure(expr);
            return expr;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationConfiguration"/> class using a configuration expression instance.
        /// </summary>
        /// <param name="expression">The configuration expression.</param>
        public MigrationConfiguration(MigrationConfigurationExpression expression)
        {
            Profiles = expression.Profiles;
        }

    }
}
