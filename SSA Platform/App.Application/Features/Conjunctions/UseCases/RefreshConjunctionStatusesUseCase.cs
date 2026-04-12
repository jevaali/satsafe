using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Entities.Enums;

namespace App.Application.Features.Conjunctions.UseCases;

public class RefreshConjunctionStatusesUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public RefreshConjunctionStatusesUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid operatorId, Guid userId)
    {
        var satellites = await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunctions = await _unitOfWork.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);

        foreach (var conjunction in conjunctions)
        {
            var previousStatus = conjunction.Status;
            conjunction.RefreshStatus();

            // Create a Critical alert when an event becomes Imminent for the first time
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
                    await _unitOfWork.Alerts.AddAsync(alert);
                }
            }

            await _unitOfWork.ConjunctionEvents.UpdateAsync(conjunction);
        }

        await _unitOfWork.CommitAsync();
    }
}
