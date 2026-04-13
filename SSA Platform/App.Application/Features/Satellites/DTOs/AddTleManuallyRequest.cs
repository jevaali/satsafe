using System.ComponentModel.DataAnnotations;

namespace App.Application.Features.Satellites.DTOs;

public class AddTleManuallyRequest
{
    public Guid SatelliteId { get; set; }

    [Required(ErrorMessage = "TLE Line 1 is required")]
    [StringLength(69, MinimumLength = 69, ErrorMessage = "TLE Line 1 must be exactly 69 characters")]
    [Display(Name = "TLE Line 1")]
    public string RawLine1 { get; set; } = string.Empty;

    [Required(ErrorMessage = "TLE Line 2 is required")]
    [StringLength(69, MinimumLength = 69, ErrorMessage = "TLE Line 2 must be exactly 69 characters")]
    [Display(Name = "TLE Line 2")]
    public string RawLine2 { get; set; } = string.Empty;
}
