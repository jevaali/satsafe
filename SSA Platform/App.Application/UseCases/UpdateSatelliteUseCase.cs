using App.Application.DTO;
using App.Application.Interfaces;
using App.Domain.ValueObjects;

namespace App.Application.UseCases;

public class UpdateSatelliteUseCase
{
    private readonly IUnitOfWork _uow;

    public UpdateSatelliteUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task Execute(Guid satelliteId, UpdateSatelliteRequest request, string updatedBy)
    {
        var satellite = await _uow.Satellites.GetByIdAsync(satelliteId);
        if (satellite == null || satellite.DeletedAt != null)
            throw new Exception("Satellite not found or deleted");

        var op = await _uow.Operators.GetByIdAsync(request.OperatorId);
        if (op == null)
            throw new Exception("Operator not found");

        satellite.NoradId = request.NoradId!.Value;

        var orbitParams = new OrbitParameters(
            request.SemiMajorAxis!.Value,
            request.Eccentricity!.Value,
            request.Inclination!.Value,
            request.RAAN!.Value,
            request.ArgumentOfPerigee!.Value,
            request.MeanAnomaly!.Value
        );

        satellite.UpdateDetails(request.Name, request.LaunchDate, op, orbitParams, updatedBy);

        await _uow.Satellites.UpdateAsync(satellite);
        await _uow.CommitAsync();
    }
}