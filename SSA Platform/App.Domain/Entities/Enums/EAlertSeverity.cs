using System.ComponentModel.DataAnnotations;

namespace App.Domain.Entities.Enums;

public enum EAlertSeverity
{
    [Display(
        Name = nameof(Resources.Enums.Enums.EAlertSeverity_Low),
        ResourceType = typeof(App.Resources.Enums.Enums)
    )]
    Low,

    [Display(
        Name = nameof(Resources.Enums.Enums.EAlertSeverity_Medium),
        ResourceType = typeof(App.Resources.Enums.Enums)
    )]
    Medium,
    
    [Display(
        Name = nameof(Resources.Enums.Enums.EAlertSeverity_High),
        ResourceType = typeof(App.Resources.Enums.Enums)
    )]
    High,
    
    [Display(
        Name = nameof(Resources.Enums.Enums.EAlertSeverity_Critical),
        ResourceType = typeof(App.Resources.Enums.Enums)
    )]
    Critical,
}

