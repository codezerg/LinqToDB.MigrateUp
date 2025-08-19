using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Default implementation of IMigrationExecutionService with comprehensive error handling.
    /// </summary>
    public class MigrationExecutionService : IMigrationExecutionService
    {
        private readonly IMigrationConfigurationValidator _validator;

        /// <summary>
        /// Initializes a new instance of the MigrationExecutionService.
        /// </summary>
        /// <param name="validator">The configuration validator to use.</param>
        public MigrationExecutionService(IMigrationConfigurationValidator? validator = null)
        {
            _validator = validator ?? new MigrationConfigurationValidator();
        }

        /// <inheritdoc/>
        public MigrationResult Execute(Migration migration, MigrationConfiguration configuration)
        {
            if (migration == null)
                throw new ArgumentNullException(nameof(migration));

            var context = new MigrationExecutionContext();
            context.Start();

            try
            {
                // Validate configuration first
                var validationResult = ValidateConfiguration(configuration);
                if (!validationResult.IsValid)
                {
                    var validationException = new MigrationException(
                        $"Configuration validation failed: {string.Join(", ", validationResult.Errors)}");
                    
                    context.RecordError(validationException);
                    return MigrationResult.Failed(validationException, context);
                }

                // Log warnings if any
                foreach (var warning in validationResult.Warnings)
                {
                    migration.Logger.LogWarning(warning);
                    context.RecordWarning(warning);
                }

                // Count total tasks
                var totalTasks = configuration.Profiles.SelectMany(p => p.Tasks).Count();
                context.TotalTasks = totalTasks;

                migration.Logger.LogInformation("Starting migration execution with {TotalTasks} tasks across {ProfileCount} profiles", totalTasks, configuration.Profiles.Count);

                // Execute each profile
                foreach (var profile in configuration.Profiles)
                {
                    foreach (var task in profile.Tasks)
                    {
                        context.CurrentTask = task;
                        context.CurrentEntityType = task.EntityType;

                        try
                        {
                            migration.Logger.LogInformation("Executing task {TaskType} for entity {EntityType}", task.GetType().Name, task.EntityType?.Name ?? "Unknown");
                            
                            task.Run(migration.MigrationProvider);
                            
                            context.RecordTaskCompleted(task);
                            migration.Logger.LogInformation("Completed task {TaskType} for entity {EntityType}", task.GetType().Name, task.EntityType?.Name ?? "Unknown");
                        }
                        catch (Exception ex)
                        {
                            var taskException = new MigrationException(
                                $"Failed to execute task {task.GetType().Name} for entity {task.EntityType?.Name ?? "Unknown"}: {ex.Message}", ex);
                            
                            context.RecordError(taskException);
                            migration.Logger.LogError(taskException, "Task execution failed");
                            
                            // For now, we'll continue with other tasks even if one fails
                            // In the future, this could be configurable behavior
                        }
                    }
                }

                context.Stop();

                if (context.HasErrors)
                {
                    migration.Logger.LogError("Migration completed with {ErrorCount} errors and {WarningCount} warnings in {ElapsedSeconds:F2} seconds", context.Errors.Count, context.Warnings.Count, context.ElapsedTime.TotalSeconds);
                    return MigrationResult.Failed(context.Errors.First(), context);
                }
                else
                {
                    migration.Logger.LogInformation("Migration completed successfully with {WarningCount} warnings in {ElapsedSeconds:F2} seconds", context.Warnings.Count, context.ElapsedTime.TotalSeconds);
                    return MigrationResult.Success(context);
                }
            }
            catch (Exception ex)
            {
                context.Stop();
                context.RecordError(ex);
                migration.Logger.LogError(ex, "Migration failed with unexpected error");
                return MigrationResult.Failed(ex, context);
            }
        }

        /// <inheritdoc/>
        public MigrationResult ExecuteForEntity<TEntity>(Migration migration, MigrationConfiguration configuration) where TEntity : class
        {
            if (migration == null)
                throw new ArgumentNullException(nameof(migration));

            var context = new MigrationExecutionContext();
            context.Start();

            try
            {
                // Validate configuration first
                var validationResult = ValidateConfiguration(configuration);
                if (!validationResult.IsValid)
                {
                    var validationException = new MigrationException(
                        $"Configuration validation failed: {string.Join(", ", validationResult.Errors)}");
                    
                    context.RecordError(validationException);
                    return MigrationResult.Failed(validationException, context);
                }

                // Log warnings if any
                foreach (var warning in validationResult.Warnings)
                {
                    migration.Logger.LogWarning(warning);
                    context.RecordWarning(warning);
                }

                var entityType = typeof(TEntity);
                context.CurrentEntityType = entityType;

                // Filter tasks for the specific entity type
                var entityTasks = configuration.Profiles
                    .SelectMany(p => p.Tasks)
                    .Where(task => task.EntityType == entityType)
                    .ToList();

                context.TotalTasks = entityTasks.Count;

                migration.Logger.LogInformation("Starting entity-specific migration for {EntityType} with {TaskCount} tasks", entityType.Name, entityTasks.Count);

                // Execute tasks for the specific entity
                foreach (var task in entityTasks)
                {
                    context.CurrentTask = task;

                    try
                    {
                        migration.Logger.LogInformation("Executing task {TaskType} for entity {EntityType}", task.GetType().Name, entityType.Name);
                        
                        task.Run(migration.MigrationProvider);
                        
                        context.RecordTaskCompleted(task);
                        migration.Logger.LogInformation("Completed task {TaskType} for entity {EntityType}", task.GetType().Name, entityType.Name);
                    }
                    catch (Exception ex)
                    {
                        var taskException = new MigrationException(
                            $"Failed to execute task {task.GetType().Name} for entity {entityType.Name}: {ex.Message}", ex);
                        
                        context.RecordError(taskException);
                        migration.Logger.LogError(taskException, "Task execution failed");
                    }
                }

                context.Stop();

                if (context.HasErrors)
                {
                    migration.Logger.LogError("Entity migration for {EntityType} completed with {ErrorCount} errors and {WarningCount} warnings in {ElapsedSeconds:F2} seconds", entityType.Name, context.Errors.Count, context.Warnings.Count, context.ElapsedTime.TotalSeconds);
                    return MigrationResult.Failed(context.Errors.First(), context);
                }
                else
                {
                    migration.Logger.LogInformation("Entity migration for {EntityType} completed successfully with {WarningCount} warnings in {ElapsedSeconds:F2} seconds", entityType.Name, context.Warnings.Count, context.ElapsedTime.TotalSeconds);
                    return MigrationResult.Success(context);
                }
            }
            catch (Exception ex)
            {
                context.Stop();
                context.RecordError(ex);
                migration.Logger.LogError(ex, "Entity migration for {EntityType} failed with unexpected error", typeof(TEntity).Name);
                return MigrationResult.Failed(ex, context);
            }
        }

        /// <inheritdoc/>
        public ValidationResult ValidateConfiguration(MigrationConfiguration configuration)
        {
            return _validator.Validate(configuration);
        }
    }
}