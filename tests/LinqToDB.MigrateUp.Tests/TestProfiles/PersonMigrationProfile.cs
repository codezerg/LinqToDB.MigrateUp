using LinqToDB.MigrateUp.Tests.TestEntities;

namespace LinqToDB.MigrateUp.Tests.TestProfiles;

public class PersonMigrationProfile : MigrationProfile
{
    public PersonMigrationProfile()
    {
        this.CreateTable<Person>();

        this.CreateIndex<Person>()
            .HasName("IX_Persons_LastName")
            .AddColumn(x => x.LastName);

        this.CreateIndex<Person>()
            .AddColumn(x => x.IsActive)
            .AddColumn(x => x.CreatedDate, ascending: false);

        this.ImportData<Person>()
            .Key(x => x.PersonId)
            .Source(GetTestPersons)
            .WhenTableCreated();
    }

    private static IEnumerable<Person> GetTestPersons()
    {
        return new[]
        {
            new Person
            {
                PersonId = 1,
                FirstName = "John",
                LastName = "Doe",
                Age = 30,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Person
            {
                PersonId = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Age = 25,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        };
    }
}