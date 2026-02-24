using Base.Domain;

namespace App.Domain;

public class Satellite : SpaceObject
{
    public string Name { get; set; } = string.Empty;
    public DateTime LaunchDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public Guid OperatorId { get; set; }
    public virtual Operator? Operator { get; set; }
}
