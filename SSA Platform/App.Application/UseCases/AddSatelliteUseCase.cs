using App.Application.DTO;
using App.Application.Interfaces;
using App.Domain;
using App.Domain.Entities;
using App.Domain.ValueObjects;

namespace App.Application.UseCases;

public class AddSatelliteUseCase
{
    private readonly IUnitOfWork _uow;

    public AddSatelliteUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Execute(AddSatelliteRequest request)
    {
        var op = await _uow.Operators.GetByIdAsync(request.OperatorId);

        if (op == null)
            throw new Exception("Operator not found");

        var satellite = Satellite.Create(
            request.Name,
            request.LaunchDate,
            op
        );

        var orbitParams = new OrbitParameters(
            request.SemiMajorAxis,
            request.Eccentricity,
            request.Inclination,
            request.RAAN,
            request.ArgumentOfPerigee,
            request.MeanAnomaly
        );

        satellite.SetOrbit(orbitParams);

        await _uow.Satellites.AddAsync(satellite);

        await _uow.CommitAsync();

        return satellite.Id;
    }
}