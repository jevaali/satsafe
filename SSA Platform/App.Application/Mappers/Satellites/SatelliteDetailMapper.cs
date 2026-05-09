using App.Application.Common.DTOs;
using App.Application.Features.Satellites.DTOs;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Mappers.Satellites;

public class SatelliteDetailMapper : IBaseMapper<SatelliteDetailDto, Satellite>
{
    private readonly IBaseMapper<OrbitDto, Orbit> _orbitMapper;
    private readonly IBaseMapper<RiskAssessmentDto, Domain.Entities.RiskAssessment> _riskMapper;
    private readonly IBaseMapper<TleRecordSummaryDto, TleRecord> _tleMapper;

    public SatelliteDetailMapper(
        IBaseMapper<OrbitDto, Orbit> orbitMapper,
        IBaseMapper<RiskAssessmentDto, Domain.Entities.RiskAssessment> riskMapper,
        IBaseMapper<TleRecordSummaryDto, TleRecord> tleMapper)
    {
        _orbitMapper = orbitMapper;
        _riskMapper  = riskMapper;
        _tleMapper   = tleMapper;
    }

    public SatelliteDetailDto? Map(Satellite? s)
    {
        if (s is null) return null;

        return new SatelliteDetailDto
        {
            Id             = s.Id,
            Name           = s.Name.ToString(),
            NoradId        = s.NoradId,
            LaunchDate     = s.LaunchDate,
            Status         = s.Status,
            Orbit          = _orbitMapper.Map(s.Orbit),
            RiskAssessment = _riskMapper.Map(s.RiskAssessment),
            TleRecords     = s.TleRecords
                .OrderByDescending(t => t.Epoch)
                .Select(_tleMapper.Map)
                .OfType<TleRecordSummaryDto>()
                .ToList()
        };
    }

    public Satellite? Map(SatelliteDetailDto? entity) => throw new NotImplementedException();
}