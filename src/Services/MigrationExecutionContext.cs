using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Provides context and tracking information for migration execution.
    /// </summary>
    public class MigrationExecutionContext
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// Gets or sets the currently executing migration task.
        /// </summary>
        public IMigrationTask? CurrentTask { get; set; }

        /// <summary>
        /// Gets or sets the current entity type being processed.
        /// </summary>
        public Type? CurrentEntityType { get; set; }

        /// <summary>
        /// Gets the elapsed time since execution started.
        /// </summary>
        public TimeSpan ElapsedTime => _stopwatch.Elapsed;

        /// <summary>
        /// Gets the list of errors encountered during execution.
        /// </summary>
        public List<Exception> Errors { get; } = new List<Exception>();

        /// <summary>
        /// Gets the list of warnings generated during execution.
        /// </summary>
        public List<string> Warnings { get; } = new List<string>();

        /// <summary>
        /// Gets the list of completed tasks.
        /// </summary>
        public List<IMigrationTask> CompletedTasks { get; } = new List<IMigrationTask>();

        /// <summary>
        /// Gets or sets the total number of tasks to execute.
        /// </summary>
        public int TotalTasks { get; set; }

        /// <summary>
        /// Gets the number of completed tasks.
        /// </summary>
        public int CompletedTaskCount => CompletedTasks.Count;

        /// <summary>
        /// Gets whether the execution has encountered any errors.
        /// </summary>
        public bool HasErrors => Errors.Count > 0;

        /// <summary>
        /// Gets whether the execution has generated any warnings.
        /// </summary>
        public bool HasWarnings => Warnings.Count > 0;

        /// <summary>
        /// Gets whether the execution is currently running.
        /// </summary>
        public bool IsRunning => _stopwatch.IsRunning;

        /// <summary>
        /// Starts execution timing.
        /// </summary>
        public void Start()
        {
            _stopwatch.Start();
        }

        /// <summary>
        /// Stops execution timing.
        /// </summary>
        public void Stop()
        {
            _stopwatch.Stop();
        }

        /// <summary>
        /// Resets the execution context to its initial state.
        /// </summary>
        public void Reset()
        {
            _stopwatch.Reset();
            CurrentTask = null;
            CurrentEntityType = null;
            Errors.Clear();
            Warnings.Clear();
            CompletedTasks.Clear();
            TotalTasks = 0;
        }

        /// <summary>
        /// Records that a task has completed successfully.
        /// </summary>
        /// <param name="task">The completed task.</param>
        public void RecordTaskCompleted(IMigrationTask? task)
        {
            if (task != null)
            {
                CompletedTasks.Add(task);
            }
        }

        /// <summary>
        /// Records an error that occurred during execution.
        /// </summary>
        /// <param name="error">The error that occurred.</param>
        public void RecordError(Exception? error)
        {
            if (error != null)
            {
                Errors.Add(error);
            }
        }

        /// <summary>
        /// Records a warning generated during execution.
        /// </summary>
        /// <param name="warning">The warning message.</param>
        public void RecordWarning(string warning)
        {
            if (!string.IsNullOrWhiteSpace(warning))
            {
                Warnings.Add(warning);
            }
        }
    }
}