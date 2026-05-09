using App.Application.Interfaces;

namespace App.Application.Features.Conjunctions.UseCases;

public class ResolveConjunctionUseCase : BaseUseCase
{
    public ResolveConjunctionUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task ExecuteAsync(Guid conjunctionId, Guid operatorId)
    {
        var satellites = await Uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();

        var conjunction = await Uow.ConjunctionEvents.FindAsync(conjunctionId);
        if (conjunction == null || (!satelliteIds.Contains(conjunction.PrimaryObjectId) && !satelliteIds.Contains(conjunction.SecondaryObjectId)))
            throw new InvalidOperationException("Conjunction not found or not authorized");

        conjunction.MarkAsResolved();
        await Uow.ConjunctionEvents.UpdateAsync(conjunction);

        var alerts = await Uow.Alerts.GetByConjunctionIdsAsync([conjunctionId]);
        foreach (var alert in alerts.Where(a => !a.IsAcknowledged))
        {
            alert.Acknowledge();
            await Uow.Alerts.UpdateAsync(alert);
        }

        await Uow.CommitAsync();
    }
}
