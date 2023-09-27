using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp
{
    public class CreateTableExpression<Table> : IMigrationTask, ICreateTableExpression<Table> where Table : class
    {
        public MigrationProfile Profile { get; }


        public CreateTableExpression(MigrationProfile profile)
        {
            Profile = profile;
        }


        void IMigrationTask.Run(IMigrationProvider provider)
        {
            provider.CreateTable<Table>();
        }
    }
}
