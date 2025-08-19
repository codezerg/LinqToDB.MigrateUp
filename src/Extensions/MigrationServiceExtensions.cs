using System;
using Microsoft.Extensions.DependencyInjection;

namespace LinqToDB.MigrateUp.Extensions
{
    /// <summary>
    /// Extension methods for registering migration services with dependency injection.
    /// </summary>
    public static class MigrationServiceExtensions
    {
        /// <summary>
        /// Adds database migration services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">A delegate to configure the migration builder.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMigrations(this IServiceCollection services, 
            Action<MigrationBuilder> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var builder = new MigrationBuilder(services);
            configure(builder);
            return builder.Build();
        }

        /// <summary>
        /// Adds database migration services with automatic entity discovery.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="providerName">The database provider name.</param>
        /// <param name="migrateOnStartup">Whether to run migrations on application startup.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddAutoMigrations(this IServiceCollection services,
            string connectionString,
            string providerName,
            bool migrateOnStartup = true)
        {
            return services.AddMigrations(builder => builder
                .UseConnectionString(connectionString)
                .UseProvider(providerName)
                .AutoDiscoverEntities(System.Reflection.Assembly.GetCallingAssembly())
                .AutoDiscoverProfiles(System.Reflection.Assembly.GetCallingAssembly())
                .MigrateOnStartup(migrateOnStartup)
                .WithOptions(options =>
                {
                    options.EnableCaching = true;
                }));
        }

        /// <summary>
        /// Adds SQLite migration services with automatic configuration.
        /// </summary>
        public static IServiceCollection AddSQLiteMigrations(this IServiceCollection services,
            string connectionString,
            bool migrateOnStartup = true)
        {
            return services.AddMigrations(builder => builder
                .UseSQLite(connectionString)
                .AutoDiscoverEntities(System.Reflection.Assembly.GetCallingAssembly())
                .AutoDiscoverProfiles(System.Reflection.Assembly.GetCallingAssembly())
                .MigrateOnStartup(migrateOnStartup));
        }

        /// <summary>
        /// Adds SQL Server migration services with automatic configuration.
        /// </summary>
        public static IServiceCollection AddSqlServerMigrations(this IServiceCollection services,
            string connectionString,
            bool migrateOnStartup = true)
        {
            return services.AddMigrations(builder => builder
                .UseSqlServer(connectionString)
                .AutoDiscoverEntities(System.Reflection.Assembly.GetCallingAssembly())
                .AutoDiscoverProfiles(System.Reflection.Assembly.GetCallingAssembly())
                .MigrateOnStartup(migrateOnStartup));
        }
    }
}