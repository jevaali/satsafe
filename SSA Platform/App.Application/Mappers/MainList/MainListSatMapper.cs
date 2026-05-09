using App.Application.Common.DTOs;
using App.Application.Features.Dashboard.DTOs;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Mappers;

public class MainListSatMapper : IBaseMapper<MainListSatDTO, Satellite>
{
    private readonly IBaseMapper<OrbitDto, Domain.Entities.Orbit> _orbitMapper;

    public MainListSatMapper(IBaseMapper<OrbitDto, Domain.Entities.Orbit> orbitMapper)
    {
        _orbitMapper = orbitMapper;
    }

    public MainListSatDTO? Map(Satellite? s)
    {
        if (s is null) return null;

        var orbit = _orbitMapper.Map(s.Orbit);
        return new MainListSatDTO
        {
            Id                = s.Id,
            Name              = s.Name,
            Operator          = s.OperatorName ?? "Unknown",
            HasOrbit          = orbit != null,
            SemiMajorAxis     = orbit?.SemiMajorAxis,
            Eccentricity      = orbit?.Eccentricity,
            Inclination       = orbit?.Inclination,
            Raan              = orbit?.RAAN,
            ArgumentOfPerigee = orbit?.ArgumentOfPerigee,
            MeanAnomaly       = orbit?.MeanAnomaly
        };
    }

    public Satellite? Map(MainListSatDTO? entity) => throw new NotImplementedException();
}