using App.Application.Interfaces;

namespace App.Application.Features.Alerts.UseCases;

public class UpdateAlertMessageUseCase : BaseUseCase
{
    public UpdateAlertMessageUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task ExecuteAsync(Guid alertId, Guid operatorId, string message)
    {
        var satellites = await Uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();

        var conjunctions = await Uow.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();

        var alert = await Uow.Alerts.FindAsync(alertId);
        if (alert == null || !conjunctionIds.Contains(alert.ConjunctionEventId))
            throw new InvalidOperationException("Alert not found or not authorized");

        alert.Message?.SetTranslation(message);

        await Uow.Alerts.UpdateMessageAsync(alert);
        await Uow.CommitAsync();
    }
}
