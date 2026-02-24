using Base.Domain;

namespace App.Domain;

public class Satellite : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int NoradId { get; set; }
    public DateTime LaunchDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    // Foreign keys
    public Guid OperatorId { get; set; }
    public Guid OrbitId { get; set; }

    // Navigation properties
    public Operator? Operator { get; set; }
    public Orbit? Orbit { get; set; }
    public List<TleRecord> TleRecords { get; set; } = new();
    public List<ConjunctionEvent> PrimaryConjunctionEvents { get; set; } = new();
    public List<ConjunctionEvent> SecondaryConjunctionEvents { get; set; } = new();
}
