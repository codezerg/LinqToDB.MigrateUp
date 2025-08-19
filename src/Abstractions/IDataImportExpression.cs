using System;
using LinqToDB.MigrateUp.Abstractions;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToDB.MigrateUp.Abstractions;

/// <summary>
/// Defines the contract for an expression that represents data import during migration tasks for a specified table.
/// </summary>
/// <typeparam name="Table">The table type.</typeparam>
public interface IDataImportExpression<Table> where Table : class
{
    /// <summary>
    /// Defines the key column(s) for data matching.
    /// </summary>
    /// <typeparam name="TColumn">The type of the key column.</typeparam>
    /// <param name="keySelector">An expression pointing to the key column in the table.</param>
    /// <returns>An instance of <see cref="IDataImportExpression{Table}"/> with the specified key column(s).</returns>
    IDataImportExpression<Table> Key<TColumn>(Expression<Func<Table, TColumn>> keySelector);

    /// <summary>
    /// Specifies the data source from which data will be imported.
    /// </summary>
    /// <param name="source">A function that returns the enumerable data source.</param>
    /// <returns>An instance of <see cref="IDataImportExpression{Table}"/> with the specified source.</returns>
    IDataImportExpression<Table> Source(Func<IEnumerable<Table>> source);

    /// <summary>
    /// Specifies that data should be imported only if the table is newly created.
    /// </summary>
    /// <returns>An instance of <see cref="IDataImportExpression{Table}"/> with this condition set.</returns>
    IDataImportExpression<Table> WhenTableCreated();

    /// <summary>
    /// Specifies that data should be imported only if the table is empty.
    /// </summary>
    /// <returns>An instance of <see cref="IDataImportExpression{Table}"/> with this condition set.</returns>
    IDataImportExpression<Table> WhenTableEmpty();
}
