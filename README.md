# CLAUDE.md

## Core Principles

You are a senior C# engineer focused on Simple, Lovable, Complete applications. Prioritize clarity, performance, and maintainable architecture.

### Code Quality
- Split files >300 lines, methods >30 lines
- Use XML docs for public APIs
- Ask before adding third-party packages
- Follow existing project structure and namespaces
- Treat compiler warnings as errors

### Development Philosophy
- Simplicity over cleverness
- Explicit over implicit
- Composition over inheritance
- Fail fast with clear error messages
- Write code a 14-year-old could understand

## Working Modes

### Planner Mode
1. Ask 4-6 clarifying questions about scope and edge cases
2. Draft step-by-step plan
3. Get approval before implementing
4. Announce completion of each phase

### Architecture Mode  
1. Ask strategic questions about scale, requirements, constraints
2. Provide tradeoff analysis with alternatives
3. Iterate on design based on feedback
4. Get approval for implementation plan

### Debug Mode
1. Identify 5-7 possible root causes
2. Narrow to 1-2 most likely culprits  
3. Add targeted logging
4. Analyze findings comprehensively
5. Remove logs after approval

## C# Standards

### Naming & Style
- Follow Microsoft coding conventions
- PascalCase for public members, camelCase for private
- Use `var` when type is obvious
- Handle nullable reference types properly

### Best Practices
- Use async/await correctly, never block with `.Result` or `.Wait()`
- Implement dependency injection and interfaces
- Use specific exception types
- Validate inputs and handle edge cases
- Use `using` statements for disposables

### Testing
- Write unit tests before implementation
- Use xUnit/NUnit/MSTest as appropriate
- Focus on business logic and critical paths
- Mock dependencies (Moq, NSubstitute)
- Add integration tests for APIs and data access


# LinqToDB.MigrateUp Library

`LinqToDB.MigrateUp` is a C# library providing schema migration utilities. It's designed to facilitate database migrations using a fluent syntax that integrates seamlessly with the LINQ to DB framework.

## Features

- **Migration Profiles**: Create reusable migration profiles to apply changes to your database structure.
- **Fluent Interface**: Construct migration tasks using a fluent syntax for creating tables, indexes, and importing data.
- **Extensible Providers**: Out of the box support for SQL Server and SQLite with the capability to add more providers as needed.

## Core Interfaces & Classes

1. **Migration**: The primary entry point for running migrations against a database.
2. **MigrationProfile**: Base class for defining migration tasks.
3. **MigrationConfiguration**: Configure migration profiles to execute.

## Quick Start

To use `LinqToDB.MigrateUp`:

1. Create a `MigrationProfile`:
    ```csharp
    using LinqToDB.Mapping;
    using LinqToDB.MigrateUp;


    [Table]
    public class Person
    {
        [PrimaryKey] public int PersonId { get; set; }

        [Column] public string FirstName { get; set; }
        [Column] public string LastName { get; set; }
        [Column] public int Age { get; set; }
        [Column] public bool IsActive { get; set; }
        [Column] public DateTime CreatedDate { get; set; }
        [Column] public DateTime? ModifiedDate { get; set; }
        [Column] public DateTime? DeletedDate { get; set; }
    }


    public class PersonProfile : MigrationProfile
    {
        public PersonProfile()
        {
            this.CreateTable<Person>();

            this.CreateIndex<Person>()
                .AddColumn(x => x.PersonId)
                .AddColumn(x => x.IsActive)
                ;

            this.CreateIndex<Person>()
                .HasName("IX_Persons_LastName")
                .AddColumn(x => x.LastName)
                ;

            this.ImportData<Person>()
                .Key(x => x.PersonId)
                .Source(() => RandomPeople())
                //.WhenTableCreated()
                //.WhenTableEmpty()
                ;
        }

        IEnumerable<Person> RandomPeople()
        {
            var random = new Random();

            yield return GenerateRandomPerson(random, 1);
            yield return GenerateRandomPerson(random, 2);
            yield return GenerateRandomPerson(random, 3);
            yield return GenerateRandomPerson(random, 4);
            yield return GenerateRandomPerson(random, 5);
        }

        static string[] firstNames = new[] { "John", "Jane", "Jack", "Jill", "James", "Jenny" };
        static string[] lastNames = new[] { "Smith", "Doe", "Johnson", "Williams", "Brown", "Jones" };

        Person GenerateRandomPerson(Random random, int id)
        {
            return new Person
            {
                PersonId = id,
                LastName = lastNames[random.Next(0, lastNames.Length)],
                FirstName = firstNames[random.Next(0, firstNames.Length)],
                Age = random.Next(18, 100),
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                DeletedDate = null,
            };
        }
    }
    ```

2. Setup the `MigrationConfiguration`:
    ```csharp
    var migrationConfiguration = new MigrationConfiguration(config =>
    {
        config.AddProfiles(typeof(Program).Assembly);
    });
    ```

3. Run migrations:
    ```csharp
    var connectionString = ...
    var dataConnection = new DataConnection(ProviderName.SqlServer, connectionString);

    var migration = new Migration(dataConnection);
    migration.Run(migrationConfiguration);
    ```

## Contribute

Contributions to the `LinqToDB.MigrateUp` library are welcome. Check the issues, fork the repository, and submit a pull request!
