using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.MigrateUp.Profiles;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.MigrateUp.Configuration;
using LinqToDB.MigrateUp.Data;
using LinqToDB.MigrateUp.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LinqToDB.MigrateUp.Extensions
{
    /// <summary>
    /// Fluent builder for configuring database migrations.
    /// </summary>
    public class MigrationBuilder
    {
        private readonly IServiceCollection _services;
        private readonly MigrationOptions _options = new();
        private readonly List<Type> _profileTypes = new();
        private string? _connectionString;
        private string? _providerName;
        private bool _autoMigrateOnStartup = false;

        /// <summary>
        /// Initializes a new instance of the MigrationBuilder class.
        /// </summary>
        public MigrationBuilder(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Sets the connection string for migrations.
        /// </summary>
        public MigrationBuilder UseConnectionString(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            return this;
        }

        /// <summary>
        /// Sets the database provider name.
        /// </summary>
        public MigrationBuilder UseProvider(string providerName)
        {
            _providerName = providerName ?? throw new ArgumentNullException(nameof(providerName));
            return this;
        }

        /// <summary>
        /// Configures SQLite as the database provider.
        /// </summary>
        public MigrationBuilder UseSQLite(string connectionString)
        {
            _providerName = ProviderName.SQLite;
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            return this;
        }

        /// <summary>
        /// Configures SQL Server as the database provider.
        /// </summary>
        public MigrationBuilder UseSqlServer(string connectionString)
        {
            _providerName = ProviderName.SqlServer;
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            return this;
        }

        /// <summary>
        /// Adds a specific migration profile.
        /// </summary>
        public MigrationBuilder AddProfile<TProfile>() where TProfile : MigrationProfile, new()
        {
            _profileTypes.Add(typeof(TProfile));
            return this;
        }

        /// <summary>
        /// Adds a specific migration profile type.
        /// </summary>
        public MigrationBuilder AddProfile(Type profileType)
        {
            if (profileType == null) throw new ArgumentNullException(nameof(profileType));
            
            if (!typeof(MigrationProfile).IsAssignableFrom(profileType))
                throw new ArgumentException($"Type {profileType.Name} must inherit from MigrationProfile");
                
            _profileTypes.Add(profileType);
            return this;
        }

        /// <summary>
        /// Automatically discovers and adds all migration profiles in the specified assembly.
        /// </summary>
        public MigrationBuilder AutoDiscoverProfiles(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var profiles = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(MigrationProfile)) 
                         && !t.IsAbstract 
                         && t.GetConstructor(Type.EmptyTypes) != null);
                         
            foreach (var profile in profiles)
            {
                _profileTypes.Add(profile);
            }
            
            return this;
        }

        /// <summary>
        /// Automatically discovers entities with Table attributes and creates migrations for them.
        /// </summary>
        public MigrationBuilder AutoDiscoverEntities(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            // Register AutoEntityMigrationProfile with the assembly to scan
            _services.AddTransient(provider =>
            {
                var profile = new AutoEntityMigrationProfile();
                profile.DiscoverEntities(assembly);
                return profile;
            });
            
            return this;
        }

        /// <summary>
        /// Configures migration options.
        /// </summary>
        public MigrationBuilder WithOptions(Action<MigrationOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            
            configure(_options);
            return this;
        }

        /// <summary>
        /// Enables automatic migration on application startup.
        /// </summary>
        public MigrationBuilder MigrateOnStartup(bool enable = true)
        {
            _autoMigrateOnStartup = enable;
            return this;
        }

        /// <summary>
        /// Builds and registers all migration services.
        /// </summary>
        public IServiceCollection Build()
        {
            // Validate configuration
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("Connection string must be configured. Use UseConnectionString(), UseSQLite(), or UseSqlServer().");
                
            if (string.IsNullOrWhiteSpace(_providerName))
                throw new InvalidOperationException("Provider must be configured. Use UseProvider(), UseSQLite(), or UseSqlServer().");

            // Register migration options
            _services.AddSingleton(_options);
            
            // Register provider factory
            _services.AddSingleton<IMigrationProviderFactory, DefaultMigrationProviderFactory>();
            
            // Register migration profiles
            foreach (var profileType in _profileTypes.Distinct())
            {
                _services.AddTransient(profileType);
                _services.AddTransient<MigrationProfile>(provider => 
                    (MigrationProfile)provider.GetRequiredService(profileType));
            }

            // Register data connection factory
            _services.AddScoped<DataConnection>(provider =>
            {
                return new DataConnection(_providerName, _connectionString);
            });

            // Register Migration service
            _services.AddScoped<Migration>(provider =>
            {
                var dataConnection = provider.GetRequiredService<DataConnection>();
                var providerFactory = provider.GetRequiredService<IMigrationProviderFactory>();
                var dataService = new LinqToDbDataConnectionService(dataConnection);
                var stateManager = new MigrationStateManager();
                var logger = provider.GetService<ILogger<Migration>>() ?? 
                            new Microsoft.Extensions.Logging.Abstractions.NullLogger<Migration>();
                var options = provider.GetRequiredService<MigrationOptions>();
                return new Migration(dataService, stateManager, providerFactory, logger, options);
            });

            // Register migration runner
            _services.AddTransient<IMigrationRunner, MigrationRunner>();

            // Register hosted service for auto-migration if enabled
            if (_autoMigrateOnStartup)
            {
                _services.AddHostedService<MigrationHostedService>();
            }

            return _services;
        }
    }
}