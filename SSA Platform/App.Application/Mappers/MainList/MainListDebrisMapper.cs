using App.Application.Common.DTOs;
using App.Application.Features.Dashboard.DTOs;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Mappers.MainList;

public class MainListDebrisMapper : IBaseMapper<MainListDebrisDTO, DebrisObject>
{
    private readonly IBaseMapper<OrbitDto, Domain.Entities.Orbit> _orbitMapper;

    public MainListDebrisMapper(IBaseMapper<OrbitDto, Domain.Entities.Orbit> orbitMapper)
    {
        _orbitMapper = orbitMapper;
    }

    public MainListDebrisDTO? Map(DebrisObject? d)
    {
        if (d is null) return null;

        var orbit = _orbitMapper.Map(d.Orbit);
        return new MainListDebrisDTO
        {
            Id                = d.Id,
            NoradId           = d.NoradId,
            SizeEstimate      = d.SizeEstimate,
            HasOrbit          = orbit != null,
            SemiMajorAxis     = orbit?.SemiMajorAxis,
            Eccentricity      = orbit?.Eccentricity,
            Inclination       = orbit?.Inclination,
            Raan              = orbit?.RAAN,
            ArgumentOfPerigee = orbit?.ArgumentOfPerigee,
            MeanAnomaly       = orbit?.MeanAnomaly
        };
    }

    public DebrisObject? Map(MainListDebrisDTO? entity) => throw new NotImplementedException();
}