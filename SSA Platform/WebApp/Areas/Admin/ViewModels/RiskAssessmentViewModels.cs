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
    public Guid     Id             { get; set; }
    public double   RiskScore      { get; set; }
    public string   ModelVersion   { get; set; } = string.Empty;
    public DateTime CalculatedAt   { get; set; }
    public Guid     SatelliteId    { get; set; }
    public string   SatelliteName  { get; set; } = string.Empty;
    public string RiskLevel => RiskScore > 0.7 ? "High" : RiskScore > 0.4 ? "Medium" : "Low";
}

public class RiskAssessmentDetailViewModel
{
    public Guid     Id             { get; set; }
    public double   RiskScore      { get; set; }
    public string   ModelVersion   { get; set; } = string.Empty;
    public DateTime CalculatedAt   { get; set; }
    public Guid     SatelliteId    { get; set; }
    public string   SatelliteName  { get; set; } = string.Empty;
    public string?  CreatedBy      { get; set; }
    public DateTime? CreatedAt     { get; set; }
    public string RiskLevel => RiskScore > 0.7 ? "High" : RiskScore > 0.4 ? "Medium" : "Low";
}

public class RiskAssessmentDeleteViewModel
{
    public Guid   Id           { get; set; }
    public double RiskScore    { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public string SatelliteName { get; set; } = string.Empty;
}
