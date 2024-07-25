using LinqToDB;
using LinqToDB.Mapping;
using LinqToDB.MigrateUp.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LinqToDB.MigrateUp.Expressions
{
    internal class CreateIndexExpression<Table> : IMigrationTask, ICreateIndexExpression<Table> where Table : class
    {
        public MigrationProfile Profile { get; }

        internal string ProvidedIndexName { get; set; }
        internal List<TableIndexColumn> Columns { get; } = new List<TableIndexColumn>();


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
                throw new ArgumentException("columnSelector");
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
            if (migration.IndexesCreated.Contains(tableIndexKey))
                return;

            provider.EnsureIndex<Table>(indexName, Columns);

            migration.IndexesCreated.Add(tableIndexKey);
        }
    }
}
