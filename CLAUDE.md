# CLAUDE.md (LLM-Condensed)

> **Purpose:** Development guidance for Claude Code when working with LinqToDB.MigrateUp
> **Role:** Senior C# engineer focused on Simple, Lovable, Complete applications

## Core Principles

**Code Quality:**
- Split files >300 lines, methods >30 lines
- Use XML docs for public APIs
- Ask before adding third-party packages
- Follow existing project structure and namespaces
- Treat compiler warnings as errors

**Development Philosophy:**
- Simplicity over cleverness
- Explicit over implicit
- Composition over inheritance
- Fail fast with clear error messages
- Write code a 14-year-old could understand

## Working Modes

**Planner Mode:**
1. Ask 4-6 clarifying questions about scope and edge cases
2. Draft step-by-step plan
3. Get approval before implementing
4. Announce completion of each phase

**Architecture Mode:**
1. Ask strategic questions about scale, requirements, constraints
2. Provide tradeoff analysis with alternatives
3. Iterate on design based on feedback
4. Get approval for implementation plan

**Debug Mode:**
1. Identify 5-7 possible root causes
2. Narrow to 1-2 most likely culprits
3. Add targeted logging
4. Analyze findings comprehensively
5. Remove logs after approval

## C# Standards

**Naming & Style:**
- Follow Microsoft coding conventions
- PascalCase for public members, camelCase for private
- Use `var` when type is obvious
- Handle nullable reference types properly

**Best Practices:**
- Use async/await correctly, never block with `.Result` or `.Wait()`
- Implement dependency injection and interfaces
- Use specific exception types
- Validate inputs and handle edge cases
- Use `using` statements for disposables

**Testing:**
- Write unit tests before implementation
- Use xUnit/NUnit/MSTest as appropriate
- Focus on business logic and critical paths
- Mock dependencies (Moq, NSubstitute)
- Add integration tests for APIs and data access

## Build & Test Commands

**Build:** `dotnet build LinqToDB.MigrateUp.sln`
**Release:** `dotnet build LinqToDB.MigrateUp.sln --configuration Release`
**Test:** `dotnet test LinqToDB.MigrateUp.sln`
**Specific Tests:** `dotnet test --filter "ClassName=MigrationTests"`

## Project Structure
- **Main:** `src/LinqToDB.MigrateUp.csproj` (.NET Standard 2.0)
- **Tests:** `tests/LinqToDB.MigrateUp.Tests/` (.NET 6.0, NUnit + FluentAssertions)
- **Dependencies:** linq2db 5.0.0

## Architecture (3-Layer)
1. **Configuration:** MigrationConfiguration → MigrationProfile → IMigrationTask
2. **Execution:** Migration class orchestrates via providers
3. **Provider:** Database-specific implementations (SqlServer, SQLite, Null)

## Key Patterns

**Provider Pattern:**
```
IMigrationProvider → MigrationProviderBase → [SqlServer|SQLite|Null]Provider
Factory: DefaultMigrationProviderFactory
```

**Expression System (Fluent API):**
```
MigrationProfile creates:
├── CreateTableExpression<T> (table creation)
├── CreateIndexExpression<T> (index management)
└── DataImportExpression<T> (data seeding)
```

**Caching:**
```
IMigrationCache → InMemoryMigrationCache (thread-safe ConcurrentDictionary)
Key: EntityType → TaskType → TaskHash (SHA256)
```

## Schema Abstraction
- **TableColumn:** Column definitions (name, datatype, nullable)
- **TableIndexColumn:** Index columns with ordering
- **Entity Mapping:** LinqToDB MappingSchema integration

## Code Conventions

**Logging:** Use IMigrationLogger methods: `WriteInfo()`, `WriteWarning()`, `WriteError()`
**Threading:** Thread-safe implementations required (ConcurrentDictionary patterns)
**Interface Design:** Check existing functionality before adding new methods
**State Management:** Providers should be stateless except Migration reference

## Migration Types

**Standard:** `migration.Run(configuration)` - All profiles/tasks
**Selective:** `migration.RunForEntity<Person>(configuration)` - Entity-specific
**Caching:** Configure via MigrationOptions (EnableCaching, SkipCachedMigrations, Cache)

## Extension Points
- **Custom Providers:** Inherit MigrationProviderBase, implement abstract methods
- **Custom Tasks:** Implement IMigrationTask with EntityType property
- **Custom Caching:** Implement IMigrationCache interface

## Entity Requirements
- LinqToDB attributes: [Table], [Column], [PrimaryKey]
- Column types resolved via ISqlBuilder.BuildDataType()
- Entity types drive schema generation and task targeting