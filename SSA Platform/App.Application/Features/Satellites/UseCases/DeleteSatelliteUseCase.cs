using App.Application.Interfaces;

namespace App.Application.Features.Satellites.UseCases;

public class DeleteSatelliteUseCase : BaseUseCase
{
    public DeleteSatelliteUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task Execute(Guid satelliteId, Guid operatorId, string deletedBy)
    {
        var satellite = await Uow.Satellites.FindAsync(satelliteId);
        if (satellite == null || satellite.DeletedAt != null)
            throw new Exception("Satellite not found or already deleted");

        if (satellite.OperatorId != operatorId)
            throw new UnauthorizedAccessException("Satellite does not belong to this operator");

        satellite.SoftDelete(deletedBy);

        await Uow.Satellites.UpdateAsync(satellite);
        await Uow.CommitAsync();
    }
}
