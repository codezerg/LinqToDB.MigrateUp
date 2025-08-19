using LinqToDB.MigrateUp.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToDB.MigrateUp.Services
{
    /// <summary>
    /// Default implementation of IExpressionBuilder for building and combining LINQ expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class ExpressionBuilder<TEntity> : IExpressionBuilder<TEntity>
    {
        private readonly ParameterExpression _parameter = Expression.Parameter(typeof(TEntity));

        /// <inheritdoc/>
        public Expression<Func<TEntity, bool>> BuildKeyMatchExpression(TEntity item, Expression<Func<TEntity, bool>> templateExpression)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            
            if (templateExpression == null)
                return null;

            var substituter = new QueryParameterSubstituter<TEntity>(_parameter);
            return substituter.SubstituteParameters(templateExpression, item);
        }

        /// <inheritdoc/>
        public Expression<Func<TEntity, bool>> CombineExpressionsWithOr(IEnumerable<Expression<Func<TEntity, bool>>> expressions)
        {
            if (expressions == null)
                return null;

            return expressions.Aggregate((Expression<Func<TEntity, bool>>)null, (current, expression) =>
            {
                if (current == null)
                    return expression;

                if (expression == null)
                    return current;

                var body = Expression.OrElse(current.Body, expression.Body);
                return Expression.Lambda<Func<TEntity, bool>>(body, current.Parameters.Single());
            });
        }

        /// <inheritdoc/>
        public Expression<Func<TEntity, bool>> CombineExpressionsWithAnd(IEnumerable<Expression<Func<TEntity, bool>>> expressions)
        {
            if (expressions == null)
                return null;

            return expressions.Aggregate((Expression<Func<TEntity, bool>>)null, (current, expression) =>
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
}