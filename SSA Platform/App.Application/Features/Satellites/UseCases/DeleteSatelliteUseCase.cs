using App.Application.Interfaces;

namespace App.Application.Features.Satellites.UseCases;

public class DeleteSatelliteUseCase
{
    private readonly IUnitOfWork _uow;

    public DeleteSatelliteUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task Execute(Guid satelliteId, Guid operatorId, string deletedBy)
    {
        var satellite = await _uow.Satellites.GetByIdAsync(satelliteId);
        if (satellite == null || satellite.DeletedAt != null)
            throw new Exception("Satellite not found or already deleted");

        if (satellite.OperatorId != operatorId)
            throw new UnauthorizedAccessException("Satellite does not belong to this operator");

        satellite.SoftDelete(deletedBy);

        await _uow.Satellites.UpdateAsync(satellite);
        await _uow.CommitAsync();
    }
}
