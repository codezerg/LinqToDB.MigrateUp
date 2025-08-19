using System;
using LinqToDB.MigrateUp.Configuration;
using LinqToDB.MigrateUp.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LinqToDB.MigrateUp.Execution;

/// <summary>
/// Default implementation of IMigrationRunner that runs database migrations.
/// </summary>
public class MigrationRunner : IMigrationRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationRunner>? _logger;

    /// <summary>
    /// Initializes a new instance of the MigrationRunner class.
    /// </summary>
    public MigrationRunner(IServiceProvider serviceProvider, ILogger<MigrationRunner>? logger = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            try
            {
                _logger?.LogInformation("Starting database migrations...");

                using var scope = _serviceProvider.CreateScope();
                var migration = scope.ServiceProvider.GetRequiredService<Migration>();
                var profiles = scope.ServiceProvider.GetServices<MigrationProfile>().ToList();

                if (!profiles.Any())
                {
                    _logger?.LogWarning("No migration profiles found. Skipping migrations.");
                    return;
                }

                var configuration = new MigrationConfiguration(config =>
                {
                    foreach (var profile in profiles)
                    {
                        config.AddProfile(profile);
                        _logger?.LogDebug("Added migration profile: {ProfileType}", profile.GetType().Name);
                    }
                });

                migration.Run(configuration);
                _logger?.LogInformation("Database migrations completed successfully.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Database migration failed");
                throw;
            }
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RunForEntityAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class
    {
        await Task.Run(() =>
        {
            try
            {
                var entityType = typeof(TEntity);
                _logger?.LogInformation("Starting database migrations for entity: {EntityType}", entityType.Name);

                using var scope = _serviceProvider.CreateScope();
                var migration = scope.ServiceProvider.GetRequiredService<Migration>();
                var profiles = scope.ServiceProvider.GetServices<MigrationProfile>().ToList();

                if (!profiles.Any())
                {
                    _logger?.LogWarning("No migration profiles found. Skipping migrations for {EntityType}.", entityType.Name);
                    return;
                }

                var configuration = new MigrationConfiguration(config =>
                {
                    foreach (var profile in profiles)
                    {
                        config.AddProfile(profile);
                    }
                });

                migration.RunForEntity<TEntity>(configuration);
                _logger?.LogInformation("Database migrations for {EntityType} completed successfully.", entityType.Name);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Database migration failed for entity: {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }, cancellationToken);
    }
}