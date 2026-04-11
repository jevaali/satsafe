namespace App.Application.DTO;

public class ConjunctionSummaryDto
{
    public Guid Id { get; init; }
    public bool IsHighRisk { get; init; }
    public DateTime TimeOfClosestApproach { get; init; }
    public Guid PrimaryObjectId { get; init; }
    public Guid SecondaryObjectId { get; init; }
    public string PrimaryObjectName { get; init; } = string.Empty;
    public string SecondaryObjectName { get; init; } = string.Empty;
    public double MissDistance { get; init; }
    public double CollisionProbability { get; init; }
}
