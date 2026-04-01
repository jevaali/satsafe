using App.Domain.ValueObjects;
using Base.Domain;

namespace App.Domain.Entities;

public abstract class SpaceObject : BaseEntityWithMetaSoftDelete
{
    public int NoradId { get; set; }
    public Guid? OrbitId { get; set; }
    public required Orbit? Orbit { get; set; }
    public ICollection<TleRecord> TleRecords { get; set; } = [];
    public ICollection<ConjunctionEvent> PrimaryConjunctionEvents { get; set; } = [];
    public ICollection<ConjunctionEvent> SecondaryConjunctionEvents { get; set; } = [];
    
    public void SetOrbit(OrbitParameters parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        if (Orbit == null)
        {
            Orbit = new Orbit(parameters)
            {
                SpaceObject = this,
                SpaceObjectId = this.Id
            };
        }
        else
        {
            Orbit.UpdateParameters(parameters);
        }
    }
    public void AddTleRecord(TleRecord record)
    {
        if (record.Epoch < DateTime.UtcNow.AddDays(-30))
            throw new InvalidOperationException("TLE is too old");

        TleRecords.Add(record);
    }
    
}


