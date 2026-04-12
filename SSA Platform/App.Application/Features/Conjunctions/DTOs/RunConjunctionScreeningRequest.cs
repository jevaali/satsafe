namespace App.Application.Features.Conjunctions.DTOs;

public class RunConjunctionScreeningRequest
{
    public double ThresholdMeters  { get; set; } = 5_000;   // 5 km default
    public double WindowSeconds    { get; set; } = 86_400;  // 24h default
    public double TimeStepSeconds  { get; set; } = 60;      // 1 min default
}
