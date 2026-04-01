using App.Domain.ValueObjects;
using Base.Domain;

namespace App.Domain.Entities;

public class Orbit : BaseEntityWithMeta
{
    public OrbitParameters Parameters { get; private set; }

    public Guid SpaceObjectId { get; set; }
    public required SpaceObject SpaceObject { get; set; }

    public Orbit(OrbitParameters parameters)
    {
        Parameters = parameters;
        CreatedBy = "system"; // Required by DB constraint
        CreatedAt = DateTime.UtcNow;
    }
    
    private Orbit() { } 

    public void UpdateParameters(OrbitParameters parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }
}
