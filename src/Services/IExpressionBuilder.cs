using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Provides services for building and combining LINQ expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IExpressionBuilder<TEntity>
    {
        /// <summary>
        /// Builds a key match expression for the specified item using a template expression.
        /// </summary>
        /// <param name="item">The item to create a match expression for.</param>
        /// <param name="templateExpression">The template expression defining the key matching logic.</param>
        /// <returns>An expression that matches the specific item.</returns>
        Expression<Func<TEntity, bool>> BuildKeyMatchExpression(TEntity item, Expression<Func<TEntity, bool>> templateExpression);

        /// <summary>
        /// Combines multiple expressions using OR logic.
        /// </summary>
        /// <param name="expressions">The expressions to combine.</param>
        /// <returns>A combined expression using OR logic, or null if no expressions provided.</returns>
        Expression<Func<TEntity, bool>> CombineExpressionsWithOr(IEnumerable<Expression<Func<TEntity, bool>>> expressions);

        /// <summary>
        /// Combines multiple expressions using AND logic.
        /// </summary>
        /// <param name="expressions">The expressions to combine.</param>
        /// <returns>A combined expression using AND logic, or null if no expressions provided.</returns>
        Expression<Func<TEntity, bool>> CombineExpressionsWithAnd(IEnumerable<Expression<Func<TEntity, bool>>> expressions);
    }
}