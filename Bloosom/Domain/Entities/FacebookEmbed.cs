namespace Bloosom.Domain.Entities;

public class FacebookEmbed : BaseEntity
{
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = "post"; // post | video
    public string Label { get; set; } = string.Empty;
    public string? EventCategory { get; set; }
    public int SortOrder { get; set; }
}

