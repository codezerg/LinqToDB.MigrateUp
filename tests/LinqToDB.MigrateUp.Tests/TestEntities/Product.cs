using LinqToDB.Mapping;

namespace LinqToDB.MigrateUp.Tests.TestEntities;

[Table("Products")]
public class Product
{
    [PrimaryKey, Identity]
    public int ProductId { get; set; }

    [Column, NotNull]
    public string Name { get; set; } = string.Empty;

    [Column, NotNull]
    public string Description { get; set; } = string.Empty;

    [Column]
    public decimal Price { get; set; }

    [Column]
    public bool IsAvailable { get; set; }

    [Column]
    public DateTime CreatedDate { get; set; }
}