using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels;

public class RiskAssessmentIndexViewModel
{
    public IEnumerable<RiskAssessmentListItemViewModel> Assessments { get; set; } = [];
    public int TotalCount    { get; set; }
    public int HighRiskCount { get; set; }
}

public class RiskAssessmentListItemViewModel
{
    public Guid   Id                   { get; set; }
    public double RiskScore            { get; set; }
    public string ModelVersion         { get; set; } = string.Empty;
    public string? ConjunctionStatus   { get; set; }
    public Guid?  ConjunctionEventId   { get; set; }
    public string RiskLevel => RiskScore > 0.7 ? "High" : RiskScore > 0.3 ? "Medium" : "Low";
}

public class RiskAssessmentDetailViewModel
{
    public Guid   Id                   { get; set; }
    public double RiskScore            { get; set; }
    public string ModelVersion         { get; set; } = string.Empty;
    public string? ConjunctionStatus   { get; set; }
    public Guid?  ConjunctionEventId   { get; set; }
    public string?   CreatedBy         { get; set; }
    public DateTime? CreatedAt         { get; set; }
    public string RiskLevel => RiskScore > 0.7 ? "High" : RiskScore > 0.3 ? "Medium" : "Low";
}

public class RiskAssessmentFormViewModel
{
    public Guid? Id { get; set; }

    [Required, Range(0, 1)]
    [Display(Name = "Risk Score (0–1)")]
    public double RiskScore { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Model Version")]
    public string ModelVersion { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Conjunction Event")]
    public Guid ConjunctionEventId { get; set; }

    // Populated server-side
    public IEnumerable<SelectListItem> ConjunctionOptions { get; set; } = [];
}

public class RiskAssessmentDeleteViewModel
{
    public Guid   Id           { get; set; }
    public double RiskScore    { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public string? ConjunctionStatus { get; set; }
}
