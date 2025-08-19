using LinqToDB;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LinqToDB.MigrateUp.Expressions;

public class CreateIndexExpression<Table> : IMigrationTask, ICreateIndexExpression<Table> where Table : class
{
    public MigrationProfile Profile { get; }

    /// <inheritdoc/>
    public Type EntityType => typeof(Table);

    public string? ProvidedIndexName { get; set; }
    public List<TableIndexColumn> Columns { get; } = new List<TableIndexColumn>();


    public CreateIndexExpression(MigrationProfile profile)
    {
        Profile = profile;
    }


    public ICreateIndexExpression<Table> HasName(string name)
    {
        ProvidedIndexName = name;
        return this;
    }


    public ICreateIndexExpression<Table> AddColumn(string name, bool ascending = true)
    {
        Columns.Add(new TableIndexColumn(name, ascending));
        return this;
    }


    public ICreateIndexExpression<Table> AddColumn<TColumn>(Expression<Func<Table, TColumn>> columnSelector, bool ascending = true)
    {
        var memberExpression = columnSelector.Body as MemberExpression;
        if (memberExpression == null)
        {
            throw new ArgumentException("Invalid column selector expression. Only simple member access is supported.", nameof(columnSelector));
        }

        // Ensure the member expression is directly on the parameter (not nested property access)
        if (memberExpression.Expression?.NodeType != ExpressionType.Parameter)
        {
            throw new ArgumentException("Invalid column selector expression. Only direct property access on the entity is supported.", nameof(columnSelector));
        }

        string name = memberExpression.Member.Name;
        AddColumn(name, ascending);
        return this;
    }


    void IMigrationTask.Run(IMigrationProvider provider)
    {
        if (!Columns.Any())
            throw new InvalidOperationException("At least one column must be specified for the index.");

        var migration = provider.Migration;

        var tableName = migration.GetEntityName<Table>();
        var indexName = ProvidedIndexName;

        if (string.IsNullOrWhiteSpace(indexName))
        {
            indexName = $"IX_{tableName}_{string.Join("_", Columns.Select(c => c.ColumnName))}";
        }

        var tableIndexKey = tableName + ":" + indexName;
        if (migration.StateManager.IsIndexCreated(tableIndexKey))
            return;

        provider.EnsureIndex<Table>(indexName, Columns);

        migration.StateManager.MarkIndexCreated(tableIndexKey);
    }
}
