using App.Application.Interfaces;

namespace App.Application.UseCases;

public class ResolveConjunctionUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public ResolveConjunctionUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid conjunctionId, Guid operatorId)
    {
        var satellites = await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        
        var conjunction = await _unitOfWork.ConjunctionEvents.GetByIdAsync(conjunctionId);
        if (conjunction == null || (!satelliteIds.Contains(conjunction.PrimaryObjectId) && !satelliteIds.Contains(conjunction.SecondaryObjectId)))
            throw new InvalidOperationException("Conjunction not found or not authorized");
        
        conjunction.MarkAsResolved();
        await _unitOfWork.CommitAsync();
    }
}
