using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Services;

namespace App.Application.Features.Conjunctions.UseCases;

public class RunConjunctionScreeningUseCase : BaseUseCase
{
    private readonly ConjunctionAnalysisService _conjunctionService;

    public RunConjunctionScreeningUseCase(IUnitOfWork uow, ConjunctionAnalysisService conjunctionService)
        : base(uow)
    {
        _conjunctionService = conjunctionService;
    }

    public async Task ExecuteAsync(Guid operatorId, Guid userId)
    {
        var satellites = await Uow.Satellites.GetByOperatorIdAsync(operatorId);
        if (satellites.Count(s => s.Orbit != null) < 2) return;

        var settings               = await Uow.NotificationSettings.GetByUserIdAsync(userId);
        var conjunctionThresholdKm = settings?.ConjunctionThresholdKm ?? 50.0;
        var alertThresholdKm       = settings?.AlertThresholdKm ?? 5.0;

        var allObjects = await Uow.SpaceObjects.GetAllWithOrbitsAsync();

        var newEvents = _conjunctionService
            .ScreenAll(allObjects.ToList(),
                thresholdMeters: conjunctionThresholdKm,
                windowSeconds:   86_400,
                timeStepSeconds: 60)
            .ToList();

        if (newEvents.Count == 0) return;

        var allExisting   = await Uow.ConjunctionEvents.AllAsync();
        var existingPairs = allExisting.Select(e => e.GetOrderedPair()).ToHashSet();

        var toSave = newEvents.Where(e => !existingPairs.Contains(e.GetOrderedPair())).ToList();
        if (toSave.Count == 0) return;

        foreach (var evt in toSave)
        {
            evt.RefreshStatus();
            Uow.ConjunctionEvents.Add(evt);
        }

        await Uow.CommitAsync();

        foreach (var alert in toSave.Select(e => e.CreateAlert(userId, alertThresholdKm)).OfType<Alert>())
        {
            Uow.Alerts.Add(alert);
        }

        await Uow.CommitAsync();
    }
}
