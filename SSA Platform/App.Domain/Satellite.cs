using System.ComponentModel.DataAnnotations;
using App.Domain.Enums;

namespace App.Domain;

public class Satellite : SpaceObject
{
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;
    public DateTime LaunchDate { get; set; }
    public ESatelliteStatus Status { get; set; }
    
    public Guid OperatorId { get; set; }
    public required Operator Operator { get; set; }
}
