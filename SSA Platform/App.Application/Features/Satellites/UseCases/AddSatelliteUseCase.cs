using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;
using App.Domain;
using App.Domain.Entities;
using App.Domain.ValueObjects;

namespace App.Application.Features.Satellites.UseCases;

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

        satellite.NoradId = request.NoradId!.Value;

        var orbitParams = new OrbitParameters(
            request.SemiMajorAxis!.Value,
            request.Eccentricity!.Value,
            request.Inclination!.Value,
            request.RAAN!.Value,
            request.ArgumentOfPerigee!.Value,
            request.MeanAnomaly!.Value
        );

        satellite.SetOrbit(orbitParams);

        await _uow.Satellites.AddAsync(satellite);

        await _uow.CommitAsync();

        return satellite.Id;
    }
}
