using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class TleRecord : BaseEntityWithMetaSoftDelete
{
    public Guid SpaceObjectId { get; set; }
    public required SpaceObject SpaceObject { get; set; }

    public DateTime Epoch { get; set; }
    public double Inclination { get; set; }
    public double RAAN { get; set; }
    public double Eccentricity { get; set; }
    public double ArgumentOfPerigee { get; set; }
    public double MeanAnomaly { get; set; }
    public double MeanMotion { get; set; }
    public int RevolutionNumber { get; set; }
    public double BSTar { get; set; }
    public double FirstDerivative { get; set; }
    public double SecondDerivative { get; set; }
    
    [MaxLength(124)]
    public string RawLine1 { get; set; } = string.Empty;
    
    [MaxLength(124)]
    public string RawLine2 { get; set; } = string.Empty;
}
