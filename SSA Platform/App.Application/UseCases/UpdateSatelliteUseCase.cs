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

    public async Task Execute(Guid satelliteId, UpdateSatelliteRequest request)
    {
        var satellite = await _uow.Satellites.GetByIdAsync(satelliteId);
        if (satellite == null || satellite.DeletedAt != null)
            throw new Exception("Satellite not found or deleted");

        var op = await _uow.Operators.GetByIdAsync(request.OperatorId);
        if (op == null)
            throw new Exception("Operator not found");

        // Convert DTO → Domain ValueObject
        var orbitParams = new OrbitParameters(
            request.SemiMajorAxis,
            request.Eccentricity,
            request.Inclination,
            request.RAAN,
            request.ArgumentOfPerigee,
            request.MeanAnomaly
        );

        // Call domain method
        satellite.UpdateDetails(request.Name, request.LaunchDate, op, orbitParams);

        await _uow.Satellites.UpdateAsync(satellite);
        await _uow.CommitAsync();
    }
}