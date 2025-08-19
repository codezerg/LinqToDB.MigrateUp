using System;

namespace LinqToDB.MigrateUp.Abstractions;

/// <summary>
/// Defines the contract for an alter column expression.
/// </summary>
/// <typeparam name="TEntity">The entity type containing the column.</typeparam>
public interface IAlterColumnExpression<TEntity> where TEntity : class
{
    /// <summary>
    /// Specifies the column to alter using a property selector.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="propertySelector">Expression selecting the property to alter.</param>
    /// <returns>The alter column expression for fluent configuration.</returns>
    IAlterColumnExpression<TEntity> Column<TProperty>(System.Linq.Expressions.Expression<Func<TEntity, TProperty>> propertySelector);

    /// <summary>
    /// Changes the data type of the column.
    /// </summary>
    /// <param name="newDataType">The new SQL data type (e.g., "VARCHAR(100)", "INT", "DECIMAL(10,2)").</param>
    /// <returns>The alter column expression for fluent configuration.</returns>
    IAlterColumnExpression<TEntity> ToType(string newDataType);

    /// <summary>
    /// Makes the column nullable.
    /// </summary>
    /// <returns>The alter column expression for fluent configuration.</returns>
    IAlterColumnExpression<TEntity> Nullable();

    /// <summary>
    /// Makes the column non-nullable.
    /// </summary>
    /// <returns>The alter column expression for fluent configuration.</returns>
    IAlterColumnExpression<TEntity> NotNullable();

    /// <summary>
    /// Sets a default value for the column.
    /// </summary>
    /// <param name="defaultValue">The default value SQL expression.</param>
    /// <returns>The alter column expression for fluent configuration.</returns>
    IAlterColumnExpression<TEntity> WithDefault(string defaultValue);
}