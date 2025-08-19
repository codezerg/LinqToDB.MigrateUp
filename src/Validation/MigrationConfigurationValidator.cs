using System;
using LinqToDB.MigrateUp.Abstractions;
using System.Linq;

namespace LinqToDB.MigrateUp.Validation
{
    /// <summary>
    /// Default implementation of IMigrationConfigurationValidator.
    /// </summary>
    public class MigrationConfigurationValidator : IMigrationConfigurationValidator
    {
        /// <inheritdoc/>
        public ValidationResult Validate(MigrationConfiguration configuration)
        {
            var result = new ValidationResult { IsValid = true };

            if (configuration == null)
            {
                result.AddError("Migration configuration cannot be null.");
                return result;
            }

            if (configuration.Profiles == null || !configuration.Profiles.Any())
            {
                result.AddError("Migration configuration must contain at least one profile.");
                return result;
            }

            foreach (var profile in configuration.Profiles)
            {
                var profileResult = ValidateProfile(profile);
                if (!profileResult.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.AddRange(profileResult.Errors);
                }
                result.Warnings.AddRange(profileResult.Warnings);
            }

            return result;
        }

        /// <inheritdoc/>
        public ValidationResult ValidateProfile(MigrationProfile profile)
        {
            var result = new ValidationResult { IsValid = true };

            if (profile == null)
            {
                result.AddError("Migration profile cannot be null.");
                return result;
            }

            if (profile.Tasks == null || !profile.Tasks.Any())
            {
                result.AddWarning("Migration profile contains no tasks and will have no effect.");
            }
            else
            {
                // Validate that all tasks have valid entity types
                foreach (var task in profile.Tasks)
                {
                    if (task == null)
                    {
                        result.AddError("Migration profile contains null task.");
                        continue;
                    }

                    if (task.EntityType == null)
                    {
                        result.AddError($"Migration task {task.GetType().Name} has null EntityType.");
                    }
                }

                // Check for duplicate entity types with the same task type (potential conflict)
                var taskGroups = profile.Tasks
                    .Where(t => t != null && t.EntityType != null)
                    .GroupBy(t => new { t.EntityType, TaskType = t.GetType() })
                    .Where(g => g.Count() > 1);

                foreach (var group in taskGroups)
                {
                    result.AddWarning($"Multiple {group.Key.TaskType.Name} tasks found for entity {group.Key.EntityType.Name}. This may cause conflicts.");
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public ValidationResult ValidateOptions(MigrationOptions options)
        {
            var result = new ValidationResult { IsValid = true };

            if (options == null)
            {
                result.AddError("Migration options cannot be null.");
                return result;
            }

            if (options.EnableCaching && options.Cache == null)
            {
                result.AddError("Caching is enabled but no cache implementation is provided.");
            }

            if (options.EnableCaching && options.SkipCachedMigrations && options.Cache == null)
            {
                result.AddError("SkipCachedMigrations is enabled but no cache implementation is provided.");
            }

            if (!options.EnableCaching && options.SkipCachedMigrations)
            {
                result.AddWarning("SkipCachedMigrations is enabled but caching is disabled. This setting will have no effect.");
            }

            return result;
        }
    }
}