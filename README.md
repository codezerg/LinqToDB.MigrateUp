# LinqToDB.MigrateUp

Database migrations for LinqToDB. One line of code.

## Install

```bash
dotnet add package LinqToDB.MigrateUp
```

## Use

```csharp
services.AddSQLiteMigrations(connectionString);
```

Done. Your database tables are created and updated automatically.

## What It Does

- Finds all your `[Table]` classes
- Creates the tables
- Updates schema changes
- Runs on startup

## Examples

### Basic Setup

```csharp
// Your entity
[Table]
public class User
{
    [PrimaryKey, Identity] public int Id { get; set; }
    [Column] public string Email { get; set; }
    [Column] public string Name { get; set; }
}

// Your Program.cs
services.AddSQLiteMigrations(connectionString);

// That's it. Table is created automatically.
```

### With SQL Server

```csharp
services.AddSqlServerMigrations(connectionString);
```

### More Control

```csharp
services.AddMigrations(builder => builder
    .UseSQLite(connectionString)
    .AutoDiscoverEntities(Assembly.GetExecutingAssembly())
    .MigrateOnStartup(true));
```

### Custom Migrations

```csharp
public class SeedDataProfile : MigrationProfile
{
    public SeedDataProfile()
    {
        this.CreateTable<Product>();
        
        this.CreateIndex<Product>()
            .HasName("IX_Product_SKU")
            .AddColumn(p => p.SKU);
            
        this.ImportData<Product>()
            .Key(p => p.Id)
            .Source(() => new[] {
                new Product { Id = 1, SKU = "PROD-001", Name = "Widget" },
                new Product { Id = 2, SKU = "PROD-002", Name = "Gadget" }
            });
    }
}

// Add it
services.AddMigrations(builder => builder
    .UseSQLite(connectionString)
    .AddProfile<SeedDataProfile>());
```

### Manual Control

```csharp
// Don't run on startup
services.AddMigrations(builder => builder
    .UseSQLite(connectionString)
    .AutoDiscoverEntities(assembly)
    .MigrateOnStartup(false));

// Run when you want
var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
await runner.RunAsync();
```

## Configuration

```csharp
services.AddMigrations(builder => builder
    .UseSQLite(connectionString)                    // Pick your database
    .AutoDiscoverEntities(assembly)                 // Find entities automatically
    .AutoDiscoverProfiles(assembly)                 // Find migration profiles
    .AddProfile<CustomProfile>()                    // Add specific profile
    .MigrateOnStartup(true)                         // Run on startup
    .WithOptions(opt => opt.EnableCaching = true)); // Configure options
```

## License

MIT