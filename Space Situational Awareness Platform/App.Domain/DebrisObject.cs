using Base.Domain;

namespace App.Domain;

public class DebrisObject : BaseEntity
{
    public int NoradId { get; set; }
    public float SizeEstimate { get; set; }
    public string RiskCategory { get; set; } = string.Empty;
    
    // Foreign key
    public Guid OrbitId { get; set; }

    // Navigation properties
    public Orbit? Orbit { get; set; }
    public List<TleRecord> TleRecords { get; set; } = new();
    public List<ConjunctionEvent> PrimaryConjunctionEvents { get; set; } = new();
    public List<ConjunctionEvent> SecondaryConjunctionEvents { get; set; } = new();
}
