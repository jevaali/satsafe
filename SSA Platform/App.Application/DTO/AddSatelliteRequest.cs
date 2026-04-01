using System.ComponentModel.DataAnnotations;

namespace App.Application.DTO;

public class AddSatelliteRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name must not exceed 200 characters")]
    [Display(Name = "Name")]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "Launch date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Launch Date")]
    public DateTime LaunchDate { get; set; }

    [Required(ErrorMessage = "Operator is required")]
    [Display(Name = "Operator")]
    public Guid OperatorId { get; set; }

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