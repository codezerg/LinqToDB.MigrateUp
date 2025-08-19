using LinqToDB.MigrateUp.Logging;
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
        public MigrationExecutionService(IMigrationConfigurationValidator validator = null)
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
                    migration.Logger.WriteWarning(warning);
                    context.RecordWarning(warning);
                }

                // Count total tasks
                var totalTasks = configuration.Profiles.SelectMany(p => p.Tasks).Count();
                context.TotalTasks = totalTasks;

                migration.Logger.WriteInfo($"Starting migration execution with {totalTasks} tasks across {configuration.Profiles.Count} profiles");

                // Execute each profile
                foreach (var profile in configuration.Profiles)
                {
                    foreach (var task in profile.Tasks)
                    {
                        context.CurrentTask = task;
                        context.CurrentEntityType = task.EntityType;

                        try
                        {
                            migration.Logger.WriteInfo($"Executing task {task.GetType().Name} for entity {task.EntityType?.Name ?? "Unknown"}");
                            
                            task.Run(migration.MigrationProvider);
                            
                            context.RecordTaskCompleted(task);
                            migration.Logger.WriteInfo($"Completed task {task.GetType().Name} for entity {task.EntityType?.Name ?? "Unknown"}");
                        }
                        catch (Exception ex)
                        {
                            var taskException = new MigrationException(
                                $"Failed to execute task {task.GetType().Name} for entity {task.EntityType?.Name ?? "Unknown"}: {ex.Message}", ex);
                            
                            context.RecordError(taskException);
                            migration.Logger.WriteError(taskException.Message);
                            
                            // For now, we'll continue with other tasks even if one fails
                            // In the future, this could be configurable behavior
                        }
                    }
                }

                context.Stop();

                if (context.HasErrors)
                {
                    migration.Logger.WriteError($"Migration completed with {context.Errors.Count} errors and {context.Warnings.Count} warnings in {context.ElapsedTime.TotalSeconds:F2} seconds");
                    return MigrationResult.Failed(context.Errors.First(), context);
                }
                else
                {
                    migration.Logger.WriteInfo($"Migration completed successfully with {context.Warnings.Count} warnings in {context.ElapsedTime.TotalSeconds:F2} seconds");
                    return MigrationResult.Success(context);
                }
            }
            catch (Exception ex)
            {
                context.Stop();
                context.RecordError(ex);
                migration.Logger.WriteError($"Migration failed with unexpected error: {ex.Message}");
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
                    migration.Logger.WriteWarning(warning);
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

                migration.Logger.WriteInfo($"Starting entity-specific migration for {entityType.Name} with {entityTasks.Count} tasks");

                // Execute tasks for the specific entity
                foreach (var task in entityTasks)
                {
                    context.CurrentTask = task;

                    try
                    {
                        migration.Logger.WriteInfo($"Executing task {task.GetType().Name} for entity {entityType.Name}");
                        
                        task.Run(migration.MigrationProvider);
                        
                        context.RecordTaskCompleted(task);
                        migration.Logger.WriteInfo($"Completed task {task.GetType().Name} for entity {entityType.Name}");
                    }
                    catch (Exception ex)
                    {
                        var taskException = new MigrationException(
                            $"Failed to execute task {task.GetType().Name} for entity {entityType.Name}: {ex.Message}", ex);
                        
                        context.RecordError(taskException);
                        migration.Logger.WriteError(taskException.Message);
                    }
                }

                context.Stop();

                if (context.HasErrors)
                {
                    migration.Logger.WriteError($"Entity migration for {entityType.Name} completed with {context.Errors.Count} errors and {context.Warnings.Count} warnings in {context.ElapsedTime.TotalSeconds:F2} seconds");
                    return MigrationResult.Failed(context.Errors.First(), context);
                }
                else
                {
                    migration.Logger.WriteInfo($"Entity migration for {entityType.Name} completed successfully with {context.Warnings.Count} warnings in {context.ElapsedTime.TotalSeconds:F2} seconds");
                    return MigrationResult.Success(context);
                }
            }
            catch (Exception ex)
            {
                context.Stop();
                context.RecordError(ex);
                migration.Logger.WriteError($"Entity migration for {typeof(TEntity).Name} failed with unexpected error: {ex.Message}");
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