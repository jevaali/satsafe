using System.ComponentModel.DataAnnotations;
using App.Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels;

public class ConjunctionEventIndexViewModel
{
    public IEnumerable<ConjunctionEventListItemViewModel> Events { get; set; } = [];
    public int TotalCount    { get; set; }
    public int HighRiskCount { get; set; }
    public int PendingCount  { get; set; }
}

public class ConjunctionEventListItemViewModel
{
    public Guid   Id                  { get; set; }
    public string PrimaryObjectName   { get; set; } = string.Empty;
    public string SecondaryObjectName { get; set; } = string.Empty;
    public DateTime Tca               { get; set; }
    public double MissDistance        { get; set; }
    public double CollisionProbability{ get; set; }
    public EConjunctionEventStatus Status { get; set; }
    public bool   IsHighRisk          { get; set; }
}

public class ConjunctionEventDetailViewModel
{
    public Guid   Id                  { get; set; }
    public string PrimaryObjectName   { get; set; } = string.Empty;
    public string SecondaryObjectName { get; set; } = string.Empty;
    public Guid   PrimaryObjectId     { get; set; }
    public Guid   SecondaryObjectId   { get; set; }
    public DateTime Tca               { get; set; }
    public double MissDistance        { get; set; }
    public double CollisionProbability{ get; set; }
    public EConjunctionEventStatus Status { get; set; }
    public bool   IsHighRisk          { get; set; }
    public string PrimaryAcknowledgement   { get; set; } = string.Empty;
    public string SecondaryAcknowledgement { get; set; } = string.Empty;
    public string?   RiskModelVersion  { get; set; }
    public double?   RiskScore         { get; set; }
    public string?   CreatedBy         { get; set; }
    public DateTime? CreatedAt         { get; set; }
}

public class ConjunctionEventFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [Display(Name = "Primary Object")]
    public Guid PrimaryObjectId { get; set; }

    [Required]
    [Display(Name = "Secondary Object")]
    public Guid SecondaryObjectId { get; set; }

    [Required]
    [Display(Name = "Time of Closest Approach (UTC)")]
    public DateTime TimeOfClosestApproach { get; set; } = DateTime.UtcNow.AddDays(1);

    [Required, Range(0, double.MaxValue)]
    [Display(Name = "Miss Distance (m)")]
    public double MissDistance { get; set; }

    [Required, Range(0, 1)]
    [Display(Name = "Collision Probability (0–1)")]
    public double CollisionProbability { get; set; }

    [Required]
    [Display(Name = "Status")]
    public EConjunctionEventStatus Status { get; set; }

    // Populated server-side
    public IEnumerable<SelectListItem> SpaceObjectOptions { get; set; } = [];
}

public class ConjunctionEventDeleteViewModel
{
    public Guid   Id                  { get; set; }
    public string PrimaryObjectName   { get; set; } = string.Empty;
    public string SecondaryObjectName { get; set; } = string.Empty;
    public DateTime Tca               { get; set; }
    public string Status              { get; set; } = string.Empty;
}
