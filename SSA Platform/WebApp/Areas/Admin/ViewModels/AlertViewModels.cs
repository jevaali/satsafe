using System.ComponentModel.DataAnnotations;
using App.Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels;

public class AlertIndexViewModel
{
    public IEnumerable<AlertListItemViewModel> Alerts { get; set; } = [];
    public int TotalCount          { get; set; }
    public int UnacknowledgedCount { get; set; }
    public int CriticalCount       { get; set; }
}

public class AlertListItemViewModel
{
    public Guid   Id                   { get; set; }
    public EAlertSeverity Severity     { get; set; }
    public string Message              { get; set; } = string.Empty;
    public string UserName             { get; set; } = string.Empty;
    public string? ConjunctionStatus   { get; set; }
    public Guid?  ConjunctionEventId   { get; set; }
    public DateTime CreatedAt          { get; set; }
    public bool   IsAcknowledged       { get; set; }
}

public class AlertDetailViewModel
{
    public Guid   Id                   { get; set; }
    public EAlertSeverity Severity     { get; set; }
    public string Message              { get; set; } = string.Empty;
    public string UserName             { get; set; } = string.Empty;
    public string UserEmail            { get; set; } = string.Empty;
    public string? ConjunctionStatus   { get; set; }
    public Guid?  ConjunctionEventId   { get; set; }
    public DateTime CreatedAt          { get; set; }
    public bool   IsAcknowledged       { get; set; }
    public string?   CreatedBy         { get; set; }
    public DateTime? UpdatedAt         { get; set; }
}

public class AlertFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [Display(Name = "Severity")]
    public EAlertSeverity Severity { get; set; }

    [Display(Name = "Acknowledged")]
    public bool IsAcknowledged { get; set; }

    [Required]
    [Display(Name = "User")]
    public Guid UserId { get; set; }

    [Required]
    [Display(Name = "Conjunction Event")]
    public Guid ConjunctionEventId { get; set; }

    // Populated server-side
    public IEnumerable<SelectListItem> UserOptions        { get; set; } = [];
    public IEnumerable<SelectListItem> ConjunctionOptions { get; set; } = [];
}

public class AlertDeleteViewModel
{
    public Guid   Id               { get; set; }
    public string Severity         { get; set; } = string.Empty;
    public string Message          { get; set; } = string.Empty;
    public string UserName         { get; set; } = string.Empty;
    public bool   IsAcknowledged   { get; set; }
}
