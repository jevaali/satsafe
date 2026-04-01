using System.ComponentModel.DataAnnotations;
using App.Domain.Entities.Enums;

namespace App.Application.DTO;

public class AddDebrisObjectRequest
{
    [Required(ErrorMessage = "Size estimate is required")]
    [Range(0.001, 10, ErrorMessage = "Size estimate must be between 0.001 and 10 meters")]
    [Display(Name = "Size Estimate (meters)")]
    public float SizeEstimate { get; set; }

    [Required(ErrorMessage = "Risk category is required")]
    [Display(Name = "Risk Category")]
    public EDebrisObjectRiskCategory RiskCategory { get; set; }

    [Required(ErrorMessage = "Semi-major axis is required")]
    [Range(6500, 50000, ErrorMessage = "Semi-major axis must be between 6,500 and 50,000 km")]
    [Display(Name = "Semi-Major Axis (km)")]
    public double SemiMajorAxis { get; set; }

    [Required(ErrorMessage = "Eccentricity is required")]
    [Range(0, 1, ErrorMessage = "Eccentricity must be between 0 and 1")]
    [Display(Name = "Eccentricity")]
    public double Eccentricity { get; set; }

    [Required(ErrorMessage = "Inclination is required")]
    [Range(0, 180, ErrorMessage = "Inclination must be between 0 and 180 degrees")]
    [Display(Name = "Inclination (degrees)")]
    public double Inclination { get; set; }

    [Required(ErrorMessage = "RAAN is required")]
    [Range(0, 360, ErrorMessage = "RAAN must be between 0 and 360 degrees")]
    [Display(Name = "RAAN (degrees)")]
    public double RAAN { get; set; }

    [Required(ErrorMessage = "Argument of perigee is required")]
    [Range(0, 360, ErrorMessage = "Argument of perigee must be between 0 and 360 degrees")]
    [Display(Name = "Argument of Perigee (degrees)")]
    public double ArgumentOfPerigee { get; set; }

    [Required(ErrorMessage = "Mean anomaly is required")]
    [Range(0, 360, ErrorMessage = "Mean anomaly must be between 0 and 360 degrees")]
    [Display(Name = "Mean Anomaly (degrees)")]
    public double MeanAnomaly { get; set; }
}
