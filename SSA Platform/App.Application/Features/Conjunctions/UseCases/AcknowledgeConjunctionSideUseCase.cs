using App.Application.Interfaces;
using App.Domain.Entities.Enums;

namespace App.Application.Features.Conjunctions.UseCases;

public class AcknowledgeConjunctionSideUseCase : BaseUseCase
{
    public AcknowledgeConjunctionSideUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task ExecuteAsync(Guid conjunctionId, Guid operatorId, EOperatorAcknowledgement state)
    {
        var conjunction = await Uow.ConjunctionEvents.FindAsync(conjunctionId)
            ?? throw new Exception("Conjunction not found.");

        var satellites = await Uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToHashSet();

        bool ownsPrimary   = satelliteIds.Contains(conjunction.PrimaryObjectId);
        bool ownsSecondary = satelliteIds.Contains(conjunction.SecondaryObjectId);

        if (!ownsPrimary && !ownsSecondary)
            throw new UnauthorizedAccessException("This conjunction does not involve any of your satellites.");

        if (ownsPrimary)
            conjunction.AcknowledgePrimarySide(state);

        if (ownsSecondary)
            conjunction.AcknowledgeSecondarySide(state);

        await Uow.ConjunctionEvents.UpdateAsync(conjunction);

        if (state == EOperatorAcknowledgement.Resolved)
        {
            foreach (var alert in conjunction.Alerts.Where(a => !a.IsAcknowledged))
            {
                alert.Acknowledge();
                await Uow.Alerts.UpdateAcknowledgedAsync(alert);
            }
        }

        await Uow.CommitAsync();
    }
}
