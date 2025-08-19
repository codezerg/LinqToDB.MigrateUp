using System;
using LinqToDB.MigrateUp.Abstractions;

namespace LinqToDB.MigrateUp.Expressions;

/// <summary>
/// Represents an expression for creating a database table.
/// </summary>
/// <typeparam name="TEntity">The type representing the database table.</typeparam>
public sealed class CreateTableExpression<TEntity> : IMigrationTask, ICreateTableExpression<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Gets the migration profile associated with this expression.
    /// </summary>
    public MigrationProfile Profile { get; }

    /// <inheritdoc/>
    public Type EntityType => typeof(TEntity);

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTableExpression{TEntity}"/> class.
    /// </summary>
    /// <param name="profile">The migration profile associated with this expression.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profile"/> is null.</exception>
    public CreateTableExpression(MigrationProfile profile)
    {
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
    }

    /// <summary>
    /// Executes the table creation task.
    /// </summary>
    /// <param name="provider">The migration provider to use for executing the task.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="provider"/> is null.</exception>
    void IMigrationTask.Run(IMigrationProvider provider)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        provider.UpdateTableSchema<TEntity>();
    }
}