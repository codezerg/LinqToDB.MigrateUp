using System.Threading;
using System.Threading.Tasks;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Defines the contract for running database migrations.
    /// </summary>
    public interface IMigrationRunner
    {
        /// <summary>
        /// Runs all configured migrations.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RunAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs migrations for a specific entity type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to run migrations for.</typeparam>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RunForEntityAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class;
    }
}