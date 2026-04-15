namespace Bloosom.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? LongDescription { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsAvailable { get; set; } = true;
}

