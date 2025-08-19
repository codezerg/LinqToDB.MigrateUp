using LinqToDB.MigrateUp.Tests.TestEntities;

namespace LinqToDB.MigrateUp.Tests.TestProfiles;

public class ProductMigrationProfile : MigrationProfile
{
    public ProductMigrationProfile()
    {
        this.CreateTable<Product>();

        this.CreateIndex<Product>()
            .HasName("IX_Products_Name")
            .AddColumn(x => x.Name);

        this.CreateIndex<Product>()
            .AddColumn(x => x.IsAvailable)
            .AddColumn(x => x.Price);

        this.ImportData<Product>()
            .Key(x => x.ProductId)
            .Source(GetTestProducts)
            .WhenTableEmpty();
    }

    private static IEnumerable<Product> GetTestProducts()
    {
        return new[]
        {
            new Product
            {
                ProductId = 1,
                Name = "Widget A",
                Description = "A useful widget",
                Price = 9.99m,
                IsAvailable = true,
                CreatedDate = DateTime.UtcNow
            },
            new Product
            {
                ProductId = 2,
                Name = "Widget B",
                Description = "Another useful widget",
                Price = 19.99m,
                IsAvailable = false,
                CreatedDate = DateTime.UtcNow
            }
        };
    }
}