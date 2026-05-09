using App.Application.Common.DTOs;
using Base.Contracts;

namespace App.Application.Mappers.Shared;

public class OrbitMapper : IBaseMapper<OrbitDto, Domain.Entities.Orbit>
{
    public OrbitDto? Map(Domain.Entities.Orbit? orbit)
    {
        if (orbit?.Parameters == null) return null;

        return new OrbitDto
        {
            SemiMajorAxis     = orbit.Parameters.SemiMajorAxis,
            Eccentricity      = orbit.Parameters.Eccentricity,
            Inclination       = orbit.Parameters.Inclination,
            RAAN              = orbit.Parameters.RAAN,
            ArgumentOfPerigee = orbit.Parameters.ArgumentOfPerigee,
            MeanAnomaly       = orbit.Parameters.MeanAnomaly
        };
    }

    public Domain.Entities.Orbit? Map(OrbitDto? entity) => throw new NotImplementedException();
}