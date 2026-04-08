using App.Application.DTO;
using App.Application.Interfaces;

namespace App.Application.UseCases;

public class GetOperatorDashboardUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOperatorDashboardUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperatorDashboardDto> ExecuteAsync(Guid userId, Guid operatorId)
    {
        var satellites = await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();

        var conjunctions = await _unitOfWork.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();

        var alerts = await _unitOfWork.Alerts.GetByConjunctionIdsAsync(conjunctionIds);

        var operatorEntity = await _unitOfWork.Operators.GetByIdAsync(operatorId);

        var activeSatellites = satellites.Count(s => s.Status == App.Domain.Entities.Enums.ESatelliteStatus.Active);
        var pendingConjunctions = conjunctions.Count(c => c.Status == App.Domain.Entities.Enums.EConjunctionEventStatus.Pending);
        var unacknowledgedAlerts = alerts.Count(a => !a.IsAcknowledged);
        var highRiskConjunctions = conjunctions.Count(c => c.IsHighRisk());

        return new OperatorDashboardDto
        {
            OperatorName = operatorEntity?.Name?.ToString() ?? "Operator",
            TotalSatellites = satellites.Count(),
            ActiveSatellites = activeSatellites,
            PendingConjunctions = pendingConjunctions,
            HighRiskConjunctions = highRiskConjunctions,
            UnacknowledgedAlerts = unacknowledgedAlerts,
            Satellites = satellites,
            Conjunctions = conjunctions,
            Alerts = alerts
        };
    }

    public async Task<IEnumerable<App.Domain.Entities.Satellite>> GetSatellitesAsync(Guid operatorId)
    {
        return await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
    }

    public async Task<App.Domain.Entities.Satellite?> GetSatelliteByIdAsync(Guid satelliteId, Guid operatorId)
    {
        var satellite = await _unitOfWork.Satellites.GetByIdAsync(satelliteId);
        if (satellite == null || satellite.OperatorId != operatorId)
            return null;
        return satellite;
    }

    public async Task<IEnumerable<App.Domain.Entities.ConjunctionEvent>> GetConjunctionsAsync(Guid operatorId)
    {
        var satellites = await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        return await _unitOfWork.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
    }

    public async Task<App.Domain.Entities.ConjunctionEvent?> GetConjunctionByIdAsync(Guid conjunctionId, Guid operatorId)
    {
        var satellites = await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunction = await _unitOfWork.ConjunctionEvents.GetByIdAsync(conjunctionId);
        if (conjunction == null || (!satelliteIds.Contains(conjunction.PrimaryObjectId) && !satelliteIds.Contains(conjunction.SecondaryObjectId)))
            return null;
        return conjunction;
    }

    public async Task<IEnumerable<App.Domain.Entities.Alert>> GetAlertsAsync(Guid operatorId)
    {
        var satellites = await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunctions = await _unitOfWork.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();
        return await _unitOfWork.Alerts.GetByConjunctionIdsAsync(conjunctionIds);
    }

    public async Task<App.Domain.Entities.Alert?> GetAlertByIdAsync(Guid alertId, Guid operatorId)
    {
        var satellites = await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunctions = await _unitOfWork.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();

        var alert = await _unitOfWork.Alerts.GetByIdAsync(alertId);
        if (alert == null || !conjunctionIds.Contains(alert.ConjunctionEventId))
            return null;
        return alert;
    }
}
