namespace Bloosom.Domain.Entities;

public class ActivityEntry : BaseEntity
{
    public string Action { get; set; } = string.Empty; // create | update | delete | login | toggle
    public string Section { get; set; } = string.Empty; // products | orders | testimonials | gallery | users | settings
    public string Detail { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserRole { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

