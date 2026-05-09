using App.Application.Features.Satellites.DTOs;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Mappers.Satellites;

public class DraftSatMapper : IBaseMapper<DraftSatelliteDto, DraftSatellite>
{
    public DraftSatelliteDto? Map(DraftSatellite? d)
    {
        if (d is null) return null;

        return new DraftSatelliteDto
        {
            Id                = d.Id,
            Name              = d.Name,
            Notes             = d.Notes,
            CreatedAt         = d.CreatedAt,
            UpdatedAt         = d.UpdatedAt,
            SemiMajorAxis     = d.Parameters.SemiMajorAxis,
            Eccentricity      = d.Parameters.Eccentricity,
            Inclination       = d.Parameters.Inclination,
            RAAN              = d.Parameters.RAAN,
            ArgumentOfPerigee = d.Parameters.ArgumentOfPerigee,
            MeanAnomaly       = d.Parameters.MeanAnomaly
        };
    }

    public DraftSatellite? Map(DraftSatelliteDto? entity) => throw new NotImplementedException();
}