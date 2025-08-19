using System;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToDB.MigrateUp.Expressions;

/// <summary>
/// Provides utility functions for substituting parameters in an expression tree with values 
/// from a specified item, while exempting a designated parameter.
/// Particularly useful for database key matching scenarios.
/// </summary>
/// <typeparam name="TSource">The type of the object from which values will be derived for substitution.</typeparam>
class QueryParameterSubstituter<TSource> : ExpressionVisitor
{
    private TSource? _substituteValuesFrom;
    private readonly ParameterExpression _exemptedParameter;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryParameterSubstituter{TSource}"/> class, 
    /// preparing it to replace instances of parameters other than the provided one with values from a specified item during substitution.
    /// </summary>
    /// <param name="exemptedParameter">The parameter in the expression that will be exempted from value replacement during substitution.</param>
    public QueryParameterSubstituter(ParameterExpression exemptedParameter)
    {
        _exemptedParameter = exemptedParameter ?? throw new ArgumentNullException(nameof(exemptedParameter));
    }

    /// <summary>
    /// Substitutes values from the specified item into the provided expression, replacing all parameters 
    /// other than the designated one with values from the item.
    /// </summary>
    /// <param name="expr">The expression in which parameters, except the designated one, will be replaced with item values.</param>
    /// <param name="substituteValuesFrom">The source item from which the values will be derived for substitution.</param>
    /// <returns>An expression where all parameters, except the designated one, have been replaced by values from the provided item.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="expr"/> or <paramref name="substituteValuesFrom"/> is null.</exception>
    public Expression<Func<TSource, bool>> SubstituteParameters(Expression<Func<TSource, bool>> expr, TSource? substituteValuesFrom)
    {
        if (expr == null)
            throw new ArgumentNullException(nameof(expr));

        if (substituteValuesFrom == null)
            throw new ArgumentNullException(nameof(substituteValuesFrom));

        _substituteValuesFrom = substituteValuesFrom;
        var body = Visit(expr.Body);
        return Expression.Lambda<Func<TSource, bool>>(body, _exemptedParameter);
    }

    /// <summary>
    /// Overrides the base VisitMember method to substitute values from the specified item 
    /// if the current node represents a parameter other than the designated one.
    /// </summary>
    /// <param name="node">The expression to visit.</param>
    /// <returns>The modified expression tree where, if applicable, the parameter is replaced by its corresponding value from the specified item.</returns>
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression != null
            && node.Expression.NodeType == ExpressionType.Parameter
            && node.Expression.Type == typeof(TSource)
            && (ParameterExpression)node.Expression != _exemptedParameter)
        {
            PropertyInfo? member = node.Member as PropertyInfo;

            if (member == null)
            {
                throw new InvalidOperationException($"The member {node.Member.Name} on type {typeof(TSource).Name} is not a property.");
            }

            object? value = member.GetValue(_substituteValuesFrom);
            return Expression.Constant(value, member.PropertyType);
        }

        return base.VisitMember(node);
    }
}
