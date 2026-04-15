using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Entities.Enums;

namespace App.Application.Features.Conjunctions.UseCases;

public class AcknowledgeConjunctionSideUseCase
{
    private readonly IUnitOfWork _uow;

    public AcknowledgeConjunctionSideUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task ExecuteAsync(Guid conjunctionId, Guid operatorId, EOperatorAcknowledgement state)
    {
        var conjunction = await _uow.ConjunctionEvents.GetByIdAsync(conjunctionId)
            ?? throw new Exception("Conjunction not found.");

        var satellites = await _uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToHashSet();

        bool ownsPrimary   = satelliteIds.Contains(conjunction.PrimaryObjectId);
        bool ownsSecondary = satelliteIds.Contains(conjunction.SecondaryObjectId);

        if (!ownsPrimary && !ownsSecondary)
            throw new UnauthorizedAccessException("This conjunction does not involve any of your satellites.");

        if (ownsPrimary)
            conjunction.AcknowledgePrimarySide(state);

        if (ownsSecondary)
            conjunction.AcknowledgeSecondarySide(state);

        await _uow.ConjunctionEvents.UpdateAsync(conjunction);

        // Acknowledge related alerts when the operator resolves their side
        if (state == EOperatorAcknowledgement.Resolved)
        {
            var alerts = await _uow.Alerts.GetByConjunctionIdsAsync([conjunctionId]);
            foreach (var alert in alerts.Where(a => !a.IsAcknowledged))
            {
                alert.Acknowledge();
                await _uow.Alerts.UpdateAsync(alert);
            }
        }

        await _uow.CommitAsync();
    }
}
