using System;
using System.Collections.Generic;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Result of a migration configuration validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets whether the validation passed.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets the list of validation errors.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets the list of validation warnings.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        public static ValidationResult Success() => new ValidationResult { IsValid = true };

        /// <summary>
        /// Creates a failed validation result with the specified error.
        /// </summary>
        /// <param name="error">The validation error.</param>
        public static ValidationResult Failed(string error)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = { error }
            };
        }

        /// <summary>
        /// Creates a failed validation result with the specified errors.
        /// </summary>
        /// <param name="errors">The validation errors.</param>
        public static ValidationResult Failed(IEnumerable<string> errors)
        {
            var result = new ValidationResult { IsValid = false };
            result.Errors.AddRange(errors);
            return result;
        }

        /// <summary>
        /// Adds an error to the validation result and marks it as invalid.
        /// </summary>
        /// <param name="error">The error message to add.</param>
        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                Errors.Add(error);
                IsValid = false;
            }
        }

        /// <summary>
        /// Adds a warning to the validation result.
        /// </summary>
        /// <param name="warning">The warning message to add.</param>
        public void AddWarning(string warning)
        {
            if (!string.IsNullOrWhiteSpace(warning))
            {
                Warnings.Add(warning);
            }
        }
    }

    /// <summary>
    /// Provides validation services for migration configurations.
    /// </summary>
    public interface IMigrationConfigurationValidator
    {
        /// <summary>
        /// Validates a migration configuration.
        /// </summary>
        /// <param name="configuration">The configuration to validate.</param>
        /// <returns>A validation result indicating success or failure with details.</returns>
        ValidationResult Validate(MigrationConfiguration configuration);

        /// <summary>
        /// Validates a single migration profile.
        /// </summary>
        /// <param name="profile">The profile to validate.</param>
        /// <returns>A validation result indicating success or failure with details.</returns>
        ValidationResult ValidateProfile(MigrationProfile profile);

        /// <summary>
        /// Validates migration options.
        /// </summary>
        /// <param name="options">The options to validate.</param>
        /// <returns>A validation result indicating success or failure with details.</returns>
        ValidationResult ValidateOptions(MigrationOptions options);
    }
}