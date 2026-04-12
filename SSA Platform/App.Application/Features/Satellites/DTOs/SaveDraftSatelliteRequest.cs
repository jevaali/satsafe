using System.ComponentModel.DataAnnotations;

namespace App.Application.Features.Satellites.DTOs;

public class SaveDraftSatelliteRequest
{
    public Guid? Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Range(6371.1, 500000, ErrorMessage = "Semi-major axis must be > Earth radius (6371 km)")]
    public double SemiMajorAxis { get; set; }

    [Range(0, 0.999, ErrorMessage = "Eccentricity must be between 0 and 0.999")]
    public double Eccentricity { get; set; }

    [Range(0, 180, ErrorMessage = "Inclination must be 0–180°")]
    public double Inclination { get; set; }

    [Range(0, 360, ErrorMessage = "RAAN must be 0–360°")]
    public double RAAN { get; set; }

    [Range(0, 360, ErrorMessage = "Argument of perigee must be 0–360°")]
    public double ArgumentOfPerigee { get; set; }

    [Range(0, 360, ErrorMessage = "Mean anomaly must be 0–360°")]
    public double MeanAnomaly { get; set; }
}
