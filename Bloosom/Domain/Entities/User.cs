namespace Bloosom.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "staff";
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
}

