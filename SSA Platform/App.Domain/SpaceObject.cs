using Base.Domain;

namespace App.Domain;

public abstract class SpaceObject : BaseEntityWithMetaSoftDelete
{
    public int NoradId { get; set; }
    public Guid? OrbitId { get; set; }
    public required Orbit? Orbit { get; set; }
    public ICollection<TleRecord> TleRecords { get; set; } = [];
    public ICollection<ConjunctionEvent> PrimaryConjunctionEvents { get; set; } = [];
    public ICollection<ConjunctionEvent> SecondaryConjunctionEvents { get; set; } = [];
    
    
    public void UpdateOrbit(Orbit orbit)
    {
        Orbit = orbit ?? throw new ArgumentNullException(nameof(orbit));
    }
    
    
    public void AddTleRecord(TleRecord record)
    {
        if (record.Epoch < DateTime.UtcNow.AddDays(-30))
            throw new InvalidOperationException("TLE is too old");

        TleRecords.Add(record);
    }
}


