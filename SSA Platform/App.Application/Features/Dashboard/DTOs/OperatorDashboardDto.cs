using App.Application.Features.Alerts.DTOs;
using App.Application.Features.Conjunctions.DTOs;
using App.Application.Features.Satellites.DTOs;

namespace App.Application.Features.Dashboard.DTOs;

public class OperatorDashboardDto
{
    public string OperatorName { get; init; } = string.Empty;
    public int TotalSatellites { get; init; }
    public int ActiveSatellites { get; init; }
    public int PendingConjunctions { get; init; }
    public int HighRiskConjunctions { get; init; }
    public int UnacknowledgedAlerts { get; init; }

    public IReadOnlyList<SatelliteSummaryDto> Satellites { get; init; } = [];
    public IReadOnlyList<ConjunctionSummaryDto> Conjunctions { get; init; } = [];
    public IReadOnlyList<AlertSummaryDto> Alerts { get; init; } = [];
}
