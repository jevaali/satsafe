using App.Domain.Enums;

namespace App.Domain;

public class DebrisObject : SpaceObject
{
    public float SizeEstimate { get; set; }
    public EDebrisObjectRiskCategory RiskCategory { get; set; }
}
