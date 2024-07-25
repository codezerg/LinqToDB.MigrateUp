using LinqToDB.Data;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.SqlQuery;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp.Providers
{
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
    }
}
