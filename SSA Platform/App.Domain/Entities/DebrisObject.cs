using App.Domain.Entities.Enums;
using App.Domain.ValueObjects;

namespace App.Domain.Entities;

public class DebrisObject : SpaceObject
{
    public float SizeEstimate { get; set; }
    public EDebrisObjectRiskCategory RiskCategory { get; set; }

    public static DebrisObject Create(
        float sizeEstimate,
        EDebrisObjectRiskCategory riskCategory)
    {
        return new DebrisObject
        {
            Id = Guid.NewGuid(),
            SizeEstimate = sizeEstimate,
            RiskCategory = riskCategory,
            Orbit = null,
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public static DebrisObject CreateFromExternalData(int dNoradId, 
        float dSizeEstimate, 
        string dRiskCategory, 
        OrbitParameters? dOrbitParameters)
    {
        var riskCategory = Enum.Parse<EDebrisObjectRiskCategory>(dRiskCategory, ignoreCase: true);
        
        var debrisObject = new DebrisObject
        {
            Id = Guid.NewGuid(),
            NoradId = dNoradId,
            SizeEstimate = dSizeEstimate,
            RiskCategory = riskCategory,
            Orbit = null,
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow
        };
        
        if (dOrbitParameters != null)
        {
            debrisObject.SetOrbit(dOrbitParameters);
        }
        
        return debrisObject;
    }
}
