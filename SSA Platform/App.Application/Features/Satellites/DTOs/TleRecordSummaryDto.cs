namespace App.Application.Features.Satellites.DTOs;

public class TleRecordSummaryDto
{
    public Guid Id { get; init; }
    public DateTime Epoch { get; init; }
    public DateTime FetchedAt { get; init; }
    public double MeanMotion { get; init; }
    public double Inclination { get; init; }
}
