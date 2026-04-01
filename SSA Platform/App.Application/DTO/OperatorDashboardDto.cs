using App.Domain.Entities;

namespace App.Application.DTO;

public class OperatorDashboardDto
{
    public string OperatorName { get; set; } = string.Empty;
    public int TotalSatellites { get; set; }
    public int ActiveSatellites { get; set; }
    public int PendingConjunctions { get; set; }
    public int HighRiskConjunctions { get; set; }
    public int UnacknowledgedAlerts { get; set; }
    
    public IEnumerable<Satellite> Satellites { get; set; } = Enumerable.Empty<Satellite>();
    public IEnumerable<ConjunctionEvent> Conjunctions { get; set; } = Enumerable.Empty<ConjunctionEvent>();
    public IEnumerable<Alert> Alerts { get; set; } = Enumerable.Empty<Alert>();
}
