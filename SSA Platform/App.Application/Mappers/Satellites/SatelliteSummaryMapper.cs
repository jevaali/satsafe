using App.Application.Common.DTOs;
using App.Application.Features.Satellites.DTOs;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Mappers.Satellites;

public class SatelliteSummaryMapper : IBaseMapper<SatelliteSummaryDto, Satellite>
{
    private readonly IBaseMapper<OrbitDto, Orbit> _orbitMapper;

    public SatelliteSummaryMapper(IBaseMapper<OrbitDto, Orbit> orbitMapper)
    {
        _orbitMapper = orbitMapper;
    }

    public SatelliteSummaryDto? Map(Satellite? s)
    {
        if (s is null) return null;

        return new SatelliteSummaryDto
        {
            Id         = s.Id,
            Name       = s.Name.ToString(),
            NoradId    = s.NoradId,
            LaunchDate = s.LaunchDate,
            Status     = s.Status,
            RiskLevel  = s.RiskAssessment?.RiskLevel,
            Orbit      = _orbitMapper.Map(s.Orbit)
        };
    }

    public Satellite? Map(SatelliteSummaryDto? entity) => throw new NotImplementedException();
}