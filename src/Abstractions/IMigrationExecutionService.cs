using System;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Result of a migration execution operation.
    /// </summary>
    public class MigrationResult
    {
        /// <summary>
        /// Gets or sets whether the migration was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the execution context with detailed information about the migration run.
        /// </summary>
        public MigrationExecutionContext? Context { get; set; }

        /// <summary>
        /// Gets or sets the exception that caused the migration to fail, if any.
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Creates a successful migration result.
        /// </summary>
        /// <param name="context">The execution context.</param>
        public static MigrationResult Success(MigrationExecutionContext? context)
        {
            return new MigrationResult
            {
                IsSuccess = true,
                Context = context ?? new MigrationExecutionContext()
            };
        }

        /// <summary>
        /// Creates a failed migration result.
        /// </summary>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="context">The execution context.</param>
        public static MigrationResult Failed(Exception exception, MigrationExecutionContext? context = null)
        {
            return new MigrationResult
            {
                IsSuccess = false,
                Exception = exception,
                Context = context ?? new MigrationExecutionContext()
            };
        }
    }

    /// <summary>
    /// Provides advanced migration execution services with enhanced error handling and context tracking.
    /// </summary>
    public interface IMigrationExecutionService
    {
        /// <summary>
        /// Executes a migration configuration with comprehensive error handling and context tracking.
        /// </summary>
        /// <param name="migration">The migration instance to use.</param>
        /// <param name="configuration">The configuration to execute.</param>
        /// <returns>A detailed migration result.</returns>
        MigrationResult Execute(Migration migration, MigrationConfiguration configuration);

        /// <summary>
        /// Executes migration tasks for a specific entity type with comprehensive error handling and context tracking.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to run migrations for.</typeparam>
        /// <param name="migration">The migration instance to use.</param>
        /// <param name="configuration">The configuration to execute.</param>
        /// <returns>A detailed migration result.</returns>
        MigrationResult ExecuteForEntity<TEntity>(Migration migration, MigrationConfiguration configuration) where TEntity : class;

        /// <summary>
        /// Validates a configuration before execution.
        /// </summary>
        /// <param name="configuration">The configuration to validate.</param>
        /// <returns>A validation result.</returns>
        ValidationResult ValidateConfiguration(MigrationConfiguration configuration);
    }
}