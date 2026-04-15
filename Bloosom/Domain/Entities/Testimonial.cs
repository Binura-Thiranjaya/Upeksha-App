namespace Bloosom.Domain.Entities;

public class Testimonial : BaseEntity
{
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string ReviewText { get; set; } = string.Empty;
    public string? CustomerImage { get; set; }
    public bool IsApproved { get; set; }
    public bool IsFeatured { get; set; }
    public string? SourcePlatform { get; set; }
    public string? SourceUrl { get; set; }
}

