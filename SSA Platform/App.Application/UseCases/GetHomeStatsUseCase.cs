using App.Application.DTO;
using App.Application.Interfaces;

namespace App.Application.UseCases;

public class GetHomeStatsUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetHomeStatsUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<HomeStatsDto> ExecuteAsync()
    {
        var totalSatellites = await _unitOfWork.Satellites.GetCountAsync();
        var activeSatellites = await _unitOfWork.Satellites.GetActiveCountAsync();
        var debrisObjects = await _unitOfWork.DebrisObjects.GetCountAsync();
        
        var topSatellites = await _unitOfWork.Satellites.GetTopAsync(10); 

        var satellitesDto = topSatellites.Select(s => new HomeStatsSatteliteDto
        {
            Name = s.Name,
            Operator = s.OperatorName ?? "Unknown"
        }).ToList();

        return new HomeStatsDto
        {
            TotalSpaceObjects = totalSatellites + debrisObjects,
            ActiveSatellites = activeSatellites,
            DebrisObjects = debrisObjects,
            Satellites = satellitesDto
        };
    }
}
