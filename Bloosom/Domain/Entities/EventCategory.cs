namespace Bloosom.Domain.Entities;

public class EventCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }

    public ICollection<EventImage> Images { get; set; } = new List<EventImage>();
}

public class EventImage : BaseEntity
{
    public Guid EventCategoryId { get; set; }
    public EventCategory? EventCategory { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
}

