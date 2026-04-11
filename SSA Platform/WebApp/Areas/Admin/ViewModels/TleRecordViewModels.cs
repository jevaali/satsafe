using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels;

public class TleRecordIndexViewModel
{
    public IEnumerable<TleRecordListItemViewModel> Records { get; set; } = [];
    public int TotalCount { get; set; }
}

public class TleRecordListItemViewModel
{
    public Guid     Id               { get; set; }
    public string   SpaceObjectNorad { get; set; } = string.Empty;
    public DateTime Epoch            { get; set; }
    public double   Inclination      { get; set; }
    public double   RAAN             { get; set; }
    public double   Eccentricity     { get; set; }
    public double   MeanMotion       { get; set; }
}

public class TleRecordDetailViewModel
{
    public Guid     Id                  { get; set; }
    public string   SpaceObjectNorad    { get; set; } = string.Empty;
    public Guid     SpaceObjectId       { get; set; }
    public DateTime Epoch               { get; set; }
    public double   Inclination         { get; set; }
    public double   RAAN                { get; set; }
    public double   Eccentricity        { get; set; }
    public double   ArgumentOfPerigee   { get; set; }
    public double   MeanAnomaly         { get; set; }
    public double   MeanMotion          { get; set; }
    public int      RevolutionNumber    { get; set; }
    public double   BSTar               { get; set; }
    public double   FirstDerivative     { get; set; }
    public double   SecondDerivative    { get; set; }
    public string   RawLine1            { get; set; } = string.Empty;
    public string   RawLine2            { get; set; } = string.Empty;
    public string?   CreatedBy          { get; set; }
    public DateTime? CreatedAt          { get; set; }
}

public class TleRecordFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [Display(Name = "Space Object")]
    public Guid SpaceObjectId { get; set; }

    [Required]
    [Display(Name = "Epoch (UTC)")]
    public DateTime Epoch { get; set; } = DateTime.UtcNow;

    [Required]
    [Display(Name = "Inclination (°)")]
    public double Inclination { get; set; }

    [Required]
    [Display(Name = "RAAN (°)")]
    public double RAAN { get; set; }

    [Required, Range(0, 1)]
    [Display(Name = "Eccentricity")]
    public double Eccentricity { get; set; }

    [Required]
    [Display(Name = "Argument of Perigee (°)")]
    public double ArgumentOfPerigee { get; set; }

    [Required]
    [Display(Name = "Mean Anomaly (°)")]
    public double MeanAnomaly { get; set; }

    [Required]
    [Display(Name = "Mean Motion (rev/day)")]
    public double MeanMotion { get; set; }

    [Display(Name = "Revolution Number")]
    public int RevolutionNumber { get; set; }

    [Display(Name = "B* Drag Term")]
    public double BSTar { get; set; }

    [Display(Name = "First Derivative")]
    public double FirstDerivative { get; set; }

    [Display(Name = "Second Derivative")]
    public double SecondDerivative { get; set; }

    [MaxLength(124)]
    [Display(Name = "TLE Line 1")]
    public string RawLine1 { get; set; } = string.Empty;

    [MaxLength(124)]
    [Display(Name = "TLE Line 2")]
    public string RawLine2 { get; set; } = string.Empty;

    // Populated server-side
    public IEnumerable<SelectListItem> SpaceObjectOptions { get; set; } = [];
}

public class TleRecordDeleteViewModel
{
    public Guid     Id               { get; set; }
    public string   SpaceObjectNorad { get; set; } = string.Empty;
    public DateTime Epoch            { get; set; }
    public double   MeanMotion       { get; set; }
}
