using System.ComponentModel.DataAnnotations;
using App.Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels;

public class SatelliteIndexViewModel
{
    public IEnumerable<SatelliteListItemViewModel> Satellites { get; set; } = [];
    public int TotalCount   { get; set; }
    public int ActiveCount  { get; set; }
    public int DeletedCount { get; set; }
}

public class SatelliteListItemViewModel
{
    public Guid   Id           { get; set; }
    public string Name         { get; set; } = string.Empty;
    public int    NoradId      { get; set; }
    public DateTime LaunchDate { get; set; }
    public ESatelliteStatus Status { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public double? SemiMajorAxis { get; set; }
    public double? Inclination   { get; set; }
    public double? RAAN          { get; set; }
    public double? ArgumentOfPerigee { get; set; }
    public double? MeanAnomaly   { get; set; }
}

public class SatelliteDetailViewModel
{
    public Guid   Id             { get; set; }
    public string Name           { get; set; } = string.Empty;
    public int    NoradId        { get; set; }
    public DateTime LaunchDate   { get; set; }
    public ESatelliteStatus Status { get; set; }
    public string OperatorName   { get; set; } = string.Empty;
    public Guid?  OperatorId     { get; set; }
    // Orbit parameters
    public double? SemiMajorAxis     { get; set; }
    public double? Eccentricity      { get; set; }
    public double? Inclination       { get; set; }
    public double? RAAN              { get; set; }
    public double? ArgumentOfPerigee { get; set; }
    public double? MeanAnomaly       { get; set; }
    // Metadata
    public string?   CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string?   UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    // TLE records
    public IEnumerable<TleRecordAdminListItem> TleRecords { get; set; } = [];
}

public class TleRecordAdminListItem
{
    public Guid     Id          { get; set; }
    public DateTime Epoch       { get; set; }
    public DateTime FetchedAt { get; set; }
    public double   MeanMotion  { get; set; }
    public double   Inclination { get; set; }
}

public class SatelliteFormViewModel
{
    public Guid? Id { get; set; }

    [Required, MaxLength(124)]
    [Display(Name = "Satellite Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "NORAD ID")]
    public int NoradId { get; set; }

    [Required]
    [Display(Name = "Launch Date")]
    [DataType(DataType.Date)]
    public DateTime LaunchDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Status")]
    public ESatelliteStatus Status { get; set; } = ESatelliteStatus.Active;

    [Display(Name = "Operator")]
    public Guid? OperatorId { get; set; }

    // Populated server-side — not bound from form
    public IEnumerable<SelectListItem> OperatorOptions { get; set; } = [];
}

public class SatelliteDeleteViewModel
{
    public Guid   Id           { get; set; }
    public string Name         { get; set; } = string.Empty;
    public int    NoradId      { get; set; }
    public DateTime LaunchDate { get; set; }
    public string Status       { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
}
