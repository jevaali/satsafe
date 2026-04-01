using App.Domain.Entities.Enums;

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
}
