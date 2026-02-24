using Base.Domain;

namespace App.Domain;

public class Satellite : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int NoradId { get; set; }
    public DateTime LaunchDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public Guid OperatorId { get; set; }
    public virtual Operator? Operator { get; set; }
    
    public Guid OrbitId { get; set; }
    public virtual Orbit? Orbit { get; set; }
    
    public ICollection<TleRecord> TleRecords { get; set; } = [];
    public ICollection<ConjunctionEvent> ConjunctionEvents { get; set; } = [];
}
