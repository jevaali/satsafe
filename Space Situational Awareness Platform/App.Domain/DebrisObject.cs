using Base.Domain;

namespace App.Domain;

public class DebrisObject : BaseEntity
{
    public int NoradId { get; set; }
    public float SizeEstimate { get; set; }
    public string RiskCategory { get; set; } = string.Empty;
    
    public Guid OrbitId { get; set; }
    public Orbit? Orbit { get; set; }
    
    public ICollection<TleRecord> TleRecords { get; set; } = [];
    public ICollection<ConjunctionEvent> PrimaryConjunctionEvents { get; set; } = [];
    public ICollection<ConjunctionEvent> SecondaryConjunctionEvents { get; set; } = [];
}
