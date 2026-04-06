using App.Application.DTO;
using App.Application.Interfaces;

namespace App.Application.UseCases;

public class GetMainListUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMainListUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MainListDTO> ExecuteAsync()
    {
        var satellites = await _unitOfWork.Satellites.GetAllAsync();
        var satelliteDtos = satellites.Select(s => new MainListSatDTO
        {
            Id           = s.Id,
            Name         = s.Name,
            Operator     = s.OperatorName ?? "Unknown",
            HasOrbit     = s.Orbit?.Parameters != null,
            SemiMajorAxis     = s.Orbit?.Parameters.SemiMajorAxis,
            Eccentricity      = s.Orbit?.Parameters.Eccentricity,
            Inclination       = s.Orbit?.Parameters.Inclination,
            Raan              = s.Orbit?.Parameters.RAAN,
            ArgumentOfPerigee = s.Orbit?.Parameters.ArgumentOfPerigee,
            MeanAnomaly       = s.Orbit?.Parameters.MeanAnomaly
        }).ToList();

        var debris = await _unitOfWork.DebrisObjects.GetAllAsync();
        var debrisDtos = debris.Select(d => new MainListDebrisDTO
        {
            Id            = d.Id,
            NoradId       = d.NoradId,
            SizeEstimate  = d.SizeEstimate,
            HasOrbit      = d.Orbit?.Parameters != null,
            SemiMajorAxis     = d.Orbit?.Parameters.SemiMajorAxis,
            Eccentricity      = d.Orbit?.Parameters.Eccentricity,
            Inclination       = d.Orbit?.Parameters.Inclination,
            Raan              = d.Orbit?.Parameters.RAAN,
            ArgumentOfPerigee = d.Orbit?.Parameters.ArgumentOfPerigee,
            MeanAnomaly       = d.Orbit?.Parameters.MeanAnomaly
        }).ToList();

        return new MainListDTO
        {
            Satellites = satelliteDtos,
            Debris     = debrisDtos
        };
    }
}