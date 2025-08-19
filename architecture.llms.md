# LinqToDB.MigrateUp Architecture (LLM-Condensed)

> **Purpose:** LLM-optimized architectural summary for code generation, analysis, and reasoning  
> **Target:** .NET Standard 2.0, integrates with LINQ to DB framework  
> **Core Function:** Database schema migration utilities with fluent API

## Core Components

### 1. Migration (Entry Point)
- **File:** `Migration.cs`
- **Role:** Primary orchestrator for migration execution
- **Dependencies:** DataConnection, MigrationOptions, IMigrationProviderFactory, IMigrationLogger
- **Key Methods:** Run(MigrationConfiguration), GetEntityName<T>()
- **State Tracking:** IndexesCreated, TablesCreated (HashSets)

### 2. Provider Pattern
- **Base:** `MigrationProviderBase.cs` (abstract)
- **Interface:** `IMigrationProvider.cs`
- **Implementations:** SqlServerProvider, SQLiteProvider, NullProvider
- **Factory:** `DefaultMigrationProviderFactory.cs`
- **Key Operations:** UpdateTableSchema<T>(), EnsureIndex<T>()

### 3. Configuration System
- **MigrationConfiguration:** Container for migration profiles
- **MigrationConfigurationExpression:** Fluent builder for configurations
- **MigrationProfile:** Base class for defining migration tasks
- **MigrationOptions:** Runtime configuration settings

### 4. Expression System (Fluent API)
- **CreateTableExpression:** Table creation operations
- **CreateIndexExpression:** Index creation with column definitions
- **DataImportExpression:** Data seeding with key mapping
- **QueryParameterSubstituter:** SQL parameter handling

### 5. Schema Abstraction
- **TableColumn:** Column definition (name, datatype, nullable)
- **TableIndexColumn:** Index column definition with ordering

### 6. Logging Infrastructure
- **IMigrationLogger:** Logging contract
- **ConsoleMigrationLogger:** Console output implementation
- **NullMigrationLogger:** No-op implementation

## Architecture Patterns

### Provider Pattern
```
IMigrationProvider (interface)
├── MigrationProviderBase (abstract)
│   ├── SqlServerProvider (concrete)
│   ├── SQLiteProvider (concrete)
│   └── NullProvider (concrete)
```

### Configuration Pattern
```
MigrationConfiguration
├── Contains: List<MigrationProfile>
├── Built via: MigrationConfigurationExpression
└── Executed by: Migration.Run()
```

### Task Execution Flow
```
Migration.Run() → Profiles → Tasks → IMigrationTask.Run(IMigrationProvider)
```

## Key Interfaces

**IMigrationProvider:** Database-specific migration operations
- UpdateTableSchema<TEntity>(): Ensure table exists/matches entity
- EnsureIndex<TEntity>(name, columns): Create/update indexes

**IMigrationTask:** Executable migration step
- Run(IMigrationProvider): Execute against provider

**IMigrationProviderFactory:** Provider creation
- CreateProvider(Migration): Return appropriate provider

## Database Operations (Provider Methods)

**Schema Detection:**
- Db_TableExists(tableName): Check table existence
- Db_GetTableColumns(tableName): Retrieve current schema
- Db_TableIndexExists(tableName, indexName): Check index existence

**Schema Modification:**
- Db_CreateTableColumn<T>(tableName, column): Add columns
- Db_AlterTableColumn(tableName, columnName, newColumn): Modify columns
- Db_CreateTableIndex/Db_DropTableIndex: Index management

## Extension Points

1. **Custom Providers:** Inherit MigrationProviderBase, implement abstract methods
2. **Custom Loggers:** Implement IMigrationLogger
3. **Custom Tasks:** Implement IMigrationTask
4. **Custom Expressions:** Extend expression system for new operations

## Dependencies
- **Primary:** linq2db 5.0.0
- **Framework:** .NET Standard 2.0
- **Integration:** Uses LinqToDB DataConnection, MappingSchema

## New Capabilities (v1.1+)

### Selective Migration
- **RunForEntity<T>()**: Execute migrations for specific entity types only
- **Entity Filtering**: MigrationProfile.GetTasksForEntity<T>() for task filtering
- **Entity Discovery**: GetEntityTypes() to discover all entities with migrations

### Caching System
- **IMigrationCache**: Interface for migration execution state caching
- **InMemoryMigrationCache**: Thread-safe in-memory cache implementation
- **Task Hashing**: Automatic hash generation for migration task configurations
- **Cache Control**: MigrationOptions.EnableCaching, SkipCachedMigrations

### Enhanced Migration Options
```csharp
var options = new MigrationOptions 
{
    EnableCaching = true,           // Enable caching (default: true)
    SkipCachedMigrations = true,    // Skip already executed migrations (default: true)
    Cache = new InMemoryMigrationCache()  // Custom cache implementation
};
```

## Usage Patterns

### Standard Migration
```csharp
migration.Run(configuration);  // Run all migrations
```

### Selective Migration
```csharp
migration.RunForEntity<Person>(configuration);  // Run only Person migrations
```

### Cache Management
```csharp
options.Cache.ClearEntityCache(typeof(Person));  // Clear cache for specific entity
options.Cache.ClearAll();  // Clear all cached state
```

## Usage Flow
1. Define entities with LinqToDB attributes
2. Create MigrationProfile with fluent expressions
3. Build MigrationConfiguration with profiles
4. Configure MigrationOptions (optional caching settings)
5. Execute via Migration.Run() or Migration.RunForEntity<T>() with DataConnection

---
*For complete examples and usage patterns, see README.md*