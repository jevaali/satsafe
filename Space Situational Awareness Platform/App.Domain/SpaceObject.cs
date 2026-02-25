using Base.Domain;

namespace App.Domain;

public abstract class SpaceObject : BaseEntity
{
    public int NoradId { get; set; }
    public Guid OrbitId { get; set; }
    public virtual Orbit? Orbit { get; set; }
    public ICollection<TleRecord> TleRecords { get; set; } = [];
    public ICollection<ConjunctionEvent> PrimaryConjunctionEvents { get; set; } = [];
    public ICollection<ConjunctionEvent> SecondaryConjunctionEvents { get; set; } = [];
}
