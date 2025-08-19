using System;
using System.Linq.Expressions;

namespace LinqToDB.MigrateUp.Abstractions;

/// <summary>
/// Defines the contract for an expression that represents the creation of an index for a specified table during migration tasks.
/// </summary>
/// <typeparam name="Table">The table type.</typeparam>
public interface ICreateIndexExpression<Table> where Table : class
{
    /// <summary>
    /// Adds a column to the index definition by its name.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <param name="ascending">Specifies if the index should sort in ascending order for this column. Default is true.</param>
    /// <returns>An instance of <see cref="ICreateIndexExpression{Table}"/> with the specified column added.</returns>
    ICreateIndexExpression<Table> AddColumn(string name, bool ascending = true);

    /// <summary>
    /// Adds a column to the index definition by using a column selector expression.
    /// </summary>
    /// <typeparam name="TColumn">The type of the column.</typeparam>
    /// <param name="columnSelector">An expression pointing to the column in the table.</param>
    /// <param name="ascending">Specifies if the index should sort in ascending order for this column. Default is true.</param>
    /// <returns>An instance of <see cref="ICreateIndexExpression{Table}"/> with the specified column added.</returns>
    ICreateIndexExpression<Table> AddColumn<TColumn>(Expression<Func<Table, TColumn>> columnSelector, bool ascending = true);

    /// <summary>
    /// Specifies the name for the index.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <returns>An instance of <see cref="ICreateIndexExpression{Table}"/> with the specified index name.</returns>
    ICreateIndexExpression<Table> HasName(string name);
}
