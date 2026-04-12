using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Services;

namespace App.Application.Features.Conjunctions.UseCases;

public class RunConjunctionScreeningUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ConjunctionAnalysisService _conjunctionService;

    public RunConjunctionScreeningUseCase(
        IUnitOfWork unitOfWork,
        ConjunctionAnalysisService conjunctionService)
    {
        _unitOfWork         = unitOfWork;
        _conjunctionService = conjunctionService;
    }

    public async Task ExecuteAsync(Guid operatorId, Guid userId)
    {
        var satellites = await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
        if (satellites.Count(s => s.Orbit != null) < 2) return;

        var settings               = await _unitOfWork.NotificationSettings.GetByUserIdAsync(userId);
        var conjunctionThresholdKm = settings?.ConjunctionThresholdKm ?? 50.0;
        var alertThresholdKm        = settings?.AlertThresholdKm ?? 5.0;

        var allObjects = await _unitOfWork.SpaceObjects.GetAllWithOrbitsAsync();

        var newEvents = _conjunctionService
            .ScreenAll(allObjects.ToList(),
                thresholdMeters: conjunctionThresholdKm,
                windowSeconds:   86_400,
                timeStepSeconds: 60)
            .ToList();

        if (newEvents.Count == 0) return;

        var allExisting   = await _unitOfWork.ConjunctionEvents.GetAllAsync();
        var existingPairs = allExisting.Select(e => e.GetOrderedPair()).ToHashSet();

        var toSave = newEvents.Where(e => !existingPairs.Contains(e.GetOrderedPair())).ToList();
        if (toSave.Count == 0) return;

        foreach (var evt in toSave)
        {
            evt.RefreshStatus();
            await _unitOfWork.ConjunctionEvents.AddAsync(evt);
        }

        await _unitOfWork.CommitAsync();

        foreach (var alert in toSave.Select(e => e.CreateAlert(userId, alertThresholdKm)).OfType<Alert>())
        {
            await _unitOfWork.Alerts.AddAsync(alert);
        }

        await _unitOfWork.CommitAsync();
    }
}
