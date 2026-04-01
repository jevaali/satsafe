using App.Domain.Entities.Enums;

namespace App.Domain.Entities;

public class DebrisObject : SpaceObject
{
    public float SizeEstimate { get; set; }
    public EDebrisObjectRiskCategory RiskCategory { get; set; }
}
