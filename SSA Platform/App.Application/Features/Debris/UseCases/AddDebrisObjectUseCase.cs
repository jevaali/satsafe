using App.Application.Features.Debris.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.ValueObjects;

namespace App.Application.Features.Debris.UseCases;

public class AddDebrisObjectUseCase
{
    private readonly IUnitOfWork _uow;

    public AddDebrisObjectUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Execute(AddDebrisObjectRequest request)
    {
        var debrisObject = DebrisObject.Create(
            request.SizeEstimate,
            request.RiskCategory
        );

        var orbitParams = new OrbitParameters(
            request.SemiMajorAxis,
            request.Eccentricity,
            request.Inclination,
            request.RAAN,
            request.ArgumentOfPerigee,
            request.MeanAnomaly
        );

        debrisObject.SetOrbit(orbitParams);

        await _uow.DebrisObjects.AddAsync(debrisObject);

        await _uow.CommitAsync();

        return debrisObject.Id;
    }
}
