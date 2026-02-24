using Microsoft.AspNetCore.Identity;

namespace App.Domain.Identity;

public class AppUser: IdentityUser<Guid> 
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public virtual ICollection<AppUserRole>? AppUserRoles { get; set; }
    
    public List<Alert>? Alerts { get; set; } = new();
    public List<NotificationSetting>? NotificationSettings { get; set; } = new();
    public List<UserOperator>? UserOperators { get; set; } = new();
}
