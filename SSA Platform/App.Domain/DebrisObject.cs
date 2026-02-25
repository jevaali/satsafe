using Base.Domain;

namespace App.Domain;

public class DebrisObject : SpaceObject
{
    public float SizeEstimate { get; set; }
    public string RiskCategory { get; set; } = string.Empty;
}
