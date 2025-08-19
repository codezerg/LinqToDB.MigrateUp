using LinqToDB.Mapping;

namespace LinqToDB.MigrateUp.Tests.TestEntities;

[Table("Persons")]
public class Person
{
    [PrimaryKey, Identity]
    public int PersonId { get; set; }

    [Column, NotNull]
    public string FirstName { get; set; } = string.Empty;

    [Column, NotNull]
    public string LastName { get; set; } = string.Empty;

    [Column]
    public int Age { get; set; }

    [Column]
    public bool IsActive { get; set; }

    [Column]
    public DateTime CreatedDate { get; set; }

    [Column, Nullable]
    public DateTime? ModifiedDate { get; set; }

    [Column, Nullable]
    public DateTime? DeletedDate { get; set; }
}