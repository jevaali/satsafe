using App.Application.Common.DTOs;
using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;

namespace App.Application.Features.Satellites.UseCases;

public class GetSatelliteByIdUseCase
{
    private readonly IUnitOfWork _uow;

    public GetSatelliteByIdUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<SatelliteDetailDto?> ExecuteAsync(Guid satelliteId, Guid operatorId)
    {
        var satellite = await _uow.Satellites.GetByIdAsync(satelliteId);

        if (satellite == null || satellite.OperatorId != operatorId)
            return null;

        return new SatelliteDetailDto
        {
            Id         = satellite.Id,
            Name       = satellite.Name.ToString(),
            NoradId    = satellite.NoradId,
            LaunchDate = satellite.LaunchDate,
            Status     = satellite.Status,
            Orbit      = satellite.Orbit == null ? null : new OrbitDto
            {
                SemiMajorAxis     = satellite.Orbit.Parameters.SemiMajorAxis,
                Eccentricity      = satellite.Orbit.Parameters.Eccentricity,
                Inclination       = satellite.Orbit.Parameters.Inclination,
                RAAN              = satellite.Orbit.Parameters.RAAN,
                ArgumentOfPerigee = satellite.Orbit.Parameters.ArgumentOfPerigee,
                MeanAnomaly       = satellite.Orbit.Parameters.MeanAnomaly
            },
            RiskAssessment = satellite.RiskAssessment == null ? null : new RiskAssessmentDto
            {
                RiskScore     = satellite.RiskAssessment.RiskScore,
                RiskLevel     = satellite.RiskAssessment.RiskLevel,
                ModelVersion  = satellite.RiskAssessment.ModelVersion,
                CalculatedAt  = satellite.RiskAssessment.CalculatedAt
            }
        };
    }
}
