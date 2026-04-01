using App.Application.Interfaces;

namespace App.Application.UseCases;

public class AcknowledgeAlertUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public AcknowledgeAlertUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid alertId, Guid operatorId)
    {
        var satellites = await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        
        var conjunctions = await _unitOfWork.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();
        
        var alert = await _unitOfWork.Alerts.GetByIdAsync(alertId);
        if (alert == null || !conjunctionIds.Contains(alert.ConjunctionEventId))
            throw new InvalidOperationException("Alert not found or not authorized");
        
        alert.Acknowledge();
        await _unitOfWork.CommitAsync();
    }
}
