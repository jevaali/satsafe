using App.Application.Features.Debris.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.ValueObjects;

namespace App.Application.Features.Debris.UseCases;

public class AddDebrisObjectUseCase : BaseUseCase
{
    public AddDebrisObjectUseCase(IUnitOfWork uow) : base(uow) { }

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

        Uow.DebrisObjects.Add(debrisObject);

        await Uow.CommitAsync();

        return debrisObject.Id;
    }
}
