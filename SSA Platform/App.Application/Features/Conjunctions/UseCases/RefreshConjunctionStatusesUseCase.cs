using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Entities.Enums;

namespace App.Application.Features.Conjunctions.UseCases;

public class RefreshConjunctionStatusesUseCase : BaseUseCase
{
    public RefreshConjunctionStatusesUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task ExecuteAsync(Guid operatorId, Guid userId)
    {
        var satellites = await Uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunctions = await Uow.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);

        foreach (var conjunction in conjunctions)
        {
            var previousStatus = conjunction.Status;
            conjunction.RefreshStatus();

            if (conjunction.Status == EConjunctionEventStatus.Imminent &&
                previousStatus != EConjunctionEventStatus.Imminent &&
                previousStatus != EConjunctionEventStatus.Resolved)
            {
                var alreadyHasImminentAlert = conjunction.Alerts
                    .Any(a => a.Severity == EAlertSeverity.Critical && !a.IsAcknowledged);

                if (!alreadyHasImminentAlert)
                {
                    var alert = new Alert
                    {
                        ConjunctionEventId = conjunction.Id,
                        UserId             = userId,
                        Severity           = EAlertSeverity.Critical,
                        Message            = $"IMMINENT: conjunction in less than 24h — miss distance {conjunction.MissDistance / 1000:F2} km",
                        IsAcknowledged     = false,
                        CreatedAt          = DateTime.UtcNow,
                        CreatedBy          = "system"
                    };
                    Uow.Alerts.Add(alert);
                }
            }

            await Uow.ConjunctionEvents.UpdateAsync(conjunction);
        }

        await Uow.CommitAsync();
    }
}
