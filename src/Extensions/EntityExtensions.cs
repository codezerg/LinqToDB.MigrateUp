using System;
using System.Linq.Expressions;
using LinqToDB.MigrateUp.Schema;

namespace LinqToDB.MigrateUp.Extensions;

/// <summary>
/// Extension methods for entity-based operations with compile-time safety.
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// Creates a type-safe table index column from a property expression.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="propertyExpression">The property expression.</param>
    /// <param name="isAscending">Whether the column is sorted ascending.</param>
    /// <returns>A table index column.</returns>
    public static TableIndexColumn CreateIndexColumn<TEntity>(
        Expression<Func<TEntity, object>> propertyExpression, 
        bool isAscending = true) where TEntity : class
    {
        var propertyName = GetPropertyName(propertyExpression);
        return new TableIndexColumn(propertyName, isAscending);
    }

    /// <summary>
    /// Gets the property name from a lambda expression.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="propertyExpression">The property expression.</param>
    /// <returns>The property name.</returns>
    public static string GetPropertyName<TEntity>(Expression<Func<TEntity, object>> propertyExpression) 
        where TEntity : class
    {
        if (propertyExpression == null)
            throw new ArgumentNullException(nameof(propertyExpression));

        var body = propertyExpression.Body;

        // Handle unary expressions (boxing)
        if (body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
        {
            body = unaryExpression.Operand;
        }

        // Handle member expressions
        if (body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        throw new ArgumentException("Expression must be a property access expression", nameof(propertyExpression));
    }
}