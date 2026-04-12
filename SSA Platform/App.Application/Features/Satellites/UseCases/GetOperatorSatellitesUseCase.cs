using App.Application.Common.DTOs;
using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;

namespace App.Application.Features.Satellites.UseCases;

public class GetOperatorSatellitesUseCase
{
    private readonly IUnitOfWork _uow;

    public GetOperatorSatellitesUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<SatelliteSummaryDto>> ExecuteAsync(Guid operatorId)
    {
        var satellites = await _uow.Satellites.GetByOperatorIdAsync(operatorId);

        return satellites.Select(s => new SatelliteSummaryDto
        {
            Id         = s.Id,
            Name       = s.Name.ToString(),
            NoradId    = s.NoradId,
            LaunchDate = s.LaunchDate,
            Status     = s.Status,
            RiskLevel  = s.RiskAssessment?.RiskLevel,
            Orbit      = s.Orbit == null ? null : new OrbitDto
            {
                SemiMajorAxis     = s.Orbit.Parameters.SemiMajorAxis,
                Eccentricity      = s.Orbit.Parameters.Eccentricity,
                Inclination       = s.Orbit.Parameters.Inclination,
                RAAN              = s.Orbit.Parameters.RAAN,
                ArgumentOfPerigee = s.Orbit.Parameters.ArgumentOfPerigee,
                MeanAnomaly       = s.Orbit.Parameters.MeanAnomaly
            }
        });
    }
}
