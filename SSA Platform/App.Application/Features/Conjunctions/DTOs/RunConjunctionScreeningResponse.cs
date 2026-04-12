namespace App.Application.Features.Conjunctions.DTOs;

public class RunConjunctionScreeningResponse
{
    public int TotalScreened    { get; init; }
    public int ConjunctionsFound { get; init; }
    public List<ConjunctionResult> Conjunctions { get; init; } = [];
}

public class ConjunctionResult
{
    public Guid   PrimaryObjectId   { get; init; }
    public Guid   SecondaryObjectId { get; init; }
    public double MissDistanceKm    { get; init; }
    public double TimeOfClosestApproachSeconds { get; init; }
    public bool   IsThreat          { get; init; }
}
