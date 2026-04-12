using App.Application.Interfaces;
using Base.Domain;

namespace App.Application.Features.Alerts.UseCases;

public class UpdateAlertMessageUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAlertMessageUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid alertId, Guid operatorId, string message)
    {
        var satellites = await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();

        var conjunctions = await _unitOfWork.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();

        var alert = await _unitOfWork.Alerts.GetByIdAsync(alertId);
        if (alert == null || !conjunctionIds.Contains(alert.ConjunctionEventId))
            throw new InvalidOperationException("Alert not found or not authorized");

        alert.Message?.SetTranslation(message);

        await _unitOfWork.Alerts.UpdateMessageAsync(alert);
        await _unitOfWork.CommitAsync();
    }
}
