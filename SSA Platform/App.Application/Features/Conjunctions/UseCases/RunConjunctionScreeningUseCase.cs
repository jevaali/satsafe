using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
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
        var withOrbits = satellites.Where(s => s.Orbit != null).ToList();
        if (withOrbits.Count < 2) return;

        var settings = await _unitOfWork.NotificationSettings.GetByUserIdAsync(userId);
        var conjunctionThresholdM = (settings?.ConjunctionThresholdKm ?? 50.0) * 1000;
        var alertThresholdM       = (settings?.AlertThresholdKm       ?? 5.0)  * 1000;

        var allObjects = await _unitOfWork.SpaceObjects.GetAllWithOrbitsAsync();

        var newEvents = _conjunctionService
            .ScreenAll(allObjects.ToList(),
                thresholdMeters: conjunctionThresholdM,
                windowSeconds:   86_400,
                timeStepSeconds: 60)
            .ToList();

        if (newEvents.Count == 0) return;

        // ✅ Load ALL existing conjunction events, not just by satellite IDs
        var allExisting = await _unitOfWork.ConjunctionEvents.GetAllAsync();

        var existingPairs = allExisting
            .Select(e => (
                Min: e.PrimaryObjectId < e.SecondaryObjectId ? e.PrimaryObjectId : e.SecondaryObjectId,
                Max: e.PrimaryObjectId < e.SecondaryObjectId ? e.SecondaryObjectId : e.PrimaryObjectId
            ))
            .ToHashSet();

        var toSave = newEvents.Where(e =>
        {
            var min = e.PrimaryObjectId < e.SecondaryObjectId ? e.PrimaryObjectId : e.SecondaryObjectId;
            var max = e.PrimaryObjectId < e.SecondaryObjectId ? e.SecondaryObjectId : e.PrimaryObjectId;
            return !existingPairs.Contains((min, max));
        }).ToList();

        Console.WriteLine($"[Screening] New unique events to save: {toSave.Count}");

        if (toSave.Count == 0) return; // ✅ exit early, nothing new

        foreach (var evt in toSave)
        {
            evt.RefreshStatus();
            await _unitOfWork.ConjunctionEvents.AddAsync(evt);
        }

        await _unitOfWork.CommitAsync();

        foreach (var evt in toSave.Where(e => e.MissDistance <= alertThresholdM))
        {
            var severity = evt.MissDistance <= alertThresholdM / 5
                ? EAlertSeverity.Critical
                : EAlertSeverity.High;

            var alert = new Alert
            {
                ConjunctionEventId = evt.Id,
                UserId             = userId,
                Severity           = severity,
                Message            = $"Conjunction detected: miss distance " +
                                     $"{evt.MissDistance / 1000:F2} km in " +
                                     $"{(evt.TimeOfClosestApproach - DateTime.UtcNow).TotalHours:F1}h",
                IsAcknowledged     = false,
                CreatedAt          = DateTime.UtcNow,
                CreatedBy          = "system"
            };
            await _unitOfWork.Alerts.AddAsync(alert);
        }

        await _unitOfWork.CommitAsync();
    }
}
