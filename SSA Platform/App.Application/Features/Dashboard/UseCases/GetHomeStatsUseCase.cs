using App.Application.Features.Dashboard.DTOs;
using App.Application.Interfaces;

namespace App.Application.Features.Dashboard.UseCases;

public class GetHomeStatsUseCase : BaseUseCase
{
    public GetHomeStatsUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task<HomeStatsDto> ExecuteAsync()
    {
        var totalSatellites  = await Uow.Satellites.GetCountAsync();
        var activeSatellites = await Uow.Satellites.GetActiveCountAsync();
        var debrisObjects    = await Uow.DebrisObjects.GetCountAsync();

        var topSatellites = await Uow.Satellites.GetTopAsync(10);

        var satellitesDto = topSatellites.Select(s => new HomeStatsSatteliteDto
        {
            Name     = s.Name,
            Operator = s.OperatorName ?? "Unknown"
        }).ToList();

        return new HomeStatsDto
        {
            TotalSpaceObjects = totalSatellites + debrisObjects,
            ActiveSatellites  = activeSatellites,
            DebrisObjects     = debrisObjects,
            Satellites        = satellitesDto
        };
    }
}
