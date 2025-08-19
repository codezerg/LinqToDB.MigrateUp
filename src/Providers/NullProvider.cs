using LinqToDB.Data;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.SqlQuery;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp.Providers;

class NullProvider : IMigrationProvider
{
    /// <inheritdoc/>
    public Migration Migration { get; }


    public NullProvider(Migration migration)
    {
        Migration = migration;
    }

    /// <inheritdoc/>
    public void UpdateTableSchema<TEntity>() where TEntity : class
    {
    }

    /// <inheritdoc/>
    public void EnsureIndex<TEntity>(string indexName, IEnumerable<TableIndexColumn> columns) where TEntity : class
    {
    }

    /// <inheritdoc/>
    public void AlterColumn<TEntity>(string tableName, string columnName, TableColumn newColumn) where TEntity : class
    {
    }

    /// <inheritdoc/>
    public void RenameColumn<TEntity>(string tableName, string oldColumnName, string newColumnName) where TEntity : class
    {
    }

    /// <inheritdoc/>
    public IDatabaseSchemaService SchemaService => throw new NotSupportedException("NullProvider does not support schema operations");
}
