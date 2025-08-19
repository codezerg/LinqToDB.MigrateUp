using LinqToDB.MigrateUp.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToDB.MigrateUp.Execution;

/// <summary>
/// Builds and combines LINQ expressions for entity queries.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class ExpressionBuilder<TEntity>
{
    private readonly ParameterExpression _parameter = Expression.Parameter(typeof(TEntity));

    /// <summary>
    /// Builds a key match expression for the given item based on a template expression.
    /// </summary>
    /// <param name="item">The item to create the expression for.</param>
    /// <param name="templateExpression">The template expression to use.</param>
    /// <returns>A key match expression or null if no template is provided.</returns>
    public Expression<Func<TEntity, bool>>? BuildKeyMatchExpression(TEntity item, Expression<Func<TEntity, bool>>? templateExpression)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        
        if (templateExpression == null)
            return null;

        var substituter = new QueryParameterSubstituter<TEntity>(_parameter);
        return substituter.SubstituteParameters(templateExpression, item);
    }

    /// <summary>
    /// Combines multiple expressions using OR logic.
    /// </summary>
    /// <param name="expressions">The expressions to combine.</param>
    /// <returns>A combined expression using OR logic, or null if no expressions provided.</returns>
    public Expression<Func<TEntity, bool>>? CombineExpressionsWithOr(IEnumerable<Expression<Func<TEntity, bool>>> expressions)
    {
        if (expressions == null)
            return null;

        return expressions.Aggregate((Expression<Func<TEntity, bool>>?)null, (current, expression) =>
        {
            if (current == null)
                return expression;

            if (expression == null)
                return current;

            var body = Expression.OrElse(current.Body, expression.Body);
            return Expression.Lambda<Func<TEntity, bool>>(body, current.Parameters.Single());
        });
    }

    /// <summary>
    /// Combines multiple expressions using AND logic.
    /// </summary>
    /// <param name="expressions">The expressions to combine.</param>
    /// <returns>A combined expression using AND logic, or null if no expressions provided.</returns>
    public Expression<Func<TEntity, bool>>? CombineExpressionsWithAnd(IEnumerable<Expression<Func<TEntity, bool>>> expressions)
    {
        if (expressions == null)
            return null;

        return expressions.Aggregate((Expression<Func<TEntity, bool>>?)null, (current, expression) =>
        {
            if (current == null)
                return expression;

            if (expression == null)
                return current;

            var body = Expression.AndAlso(current.Body, expression.Body);
            return Expression.Lambda<Func<TEntity, bool>>(body, current.Parameters.Single());
        });
    }
}