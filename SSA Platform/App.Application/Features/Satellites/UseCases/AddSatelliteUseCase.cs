using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.ValueObjects;

namespace App.Application.Features.Satellites.UseCases;

public class AddSatelliteUseCase : BaseUseCase
{
    public AddSatelliteUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task<Guid> Execute(AddSatelliteRequest request)
    {
        var op = await Uow.Operators.FindAsync(request.OperatorId);

        if (op == null)
            throw new Exception("Operator not found");

        var satellite = Satellite.Create(
            request.Name,
            request.LaunchDate,
            op
        );
        satellite.Operator = null; // prevent EF from re-inserting the untracked operator

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

        Uow.Satellites.Add(satellite);

        await Uow.CommitAsync();

        return satellite.Id;
    }
}
