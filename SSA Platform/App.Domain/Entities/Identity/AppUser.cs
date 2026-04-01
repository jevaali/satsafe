using Microsoft.AspNetCore.Identity;

namespace App.Domain.Entities.Identity;

public class AppUser: IdentityUser<Guid> 
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public virtual ICollection<AppUserRole>? AppUserRoles { get; set; }
    
    public ICollection<Alert>? Alerts { get; set; } = [];
    public ICollection<NotificationSetting>? NotificationSettings { get; set; } = [];
    public ICollection<UserOperator>? UserOperators { get; set; } = [];
}
