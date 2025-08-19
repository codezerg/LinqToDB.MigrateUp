using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// A hosted service that runs database migrations on application startup.
    /// </summary>
    public class MigrationHostedService : IHostedService
    {
        private readonly IMigrationRunner _migrationRunner;
        private readonly MigrationOptions _options;
        private readonly ILogger<MigrationHostedService>? _logger;

        /// <summary>
        /// Initializes a new instance of the MigrationHostedService class.
        /// </summary>
        public MigrationHostedService(
            IMigrationRunner migrationRunner,
            MigrationOptions options,
            ILogger<MigrationHostedService>? logger = null)
        {
            _migrationRunner = migrationRunner ?? throw new ArgumentNullException(nameof(migrationRunner));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        /// <summary>
        /// Runs migrations when the application starts.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger?.LogInformation("Starting automatic database migrations...");
                
                await _migrationRunner.RunAsync(cancellationToken);
                
                _logger?.LogInformation("Automatic database migrations completed successfully.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Automatic database migration failed");
                
                // Only throw if configured to do so, otherwise log and continue
                if (_options.ThrowOnMigrationFailure)
                {
                    throw new InvalidOperationException("Database migration failed on startup. See inner exception for details.", ex);
                }
            }
        }

        /// <summary>
        /// Called when the application is stopping.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Nothing to clean up
            return Task.CompletedTask;
        }
    }
}