namespace LinqToDB.MigrateUp.Abstractions;

/// <summary>
/// Defines the contract for an expression that represents the creation of a table during migration tasks.
/// </summary>
public interface ICreateTableExpression<Table> where Table : class
{
}
