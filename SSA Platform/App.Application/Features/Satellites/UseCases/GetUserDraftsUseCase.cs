using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Services;
using App.Domain.ValueObjects;
using Base.Contracts;

namespace App.Application.Features.Satellites.UseCases;

public class GetUserDraftsUseCase : BaseUseCase<DraftSatelliteDto, DraftSatellite>
{
    private readonly SatelliteRiskScoringService _scoringService;

    public GetUserDraftsUseCase(
        IUnitOfWork uow,
        SatelliteRiskScoringService scoringService,
        IBaseMapper<DraftSatelliteDto, DraftSatellite> mapper) : base(uow, mapper)
    {
        _scoringService = scoringService;
    }

    public async Task<IEnumerable<DraftSatelliteDto>> ExecuteAsync(Guid userId)
    {
        var drafts = await Uow.DraftSatellites.GetByUserIdAsync(userId);

        var allOrbits = (await Uow.SpaceObjects.GetAllWithOrbitsAsync())
            .Select(o => o.Orbit?.Parameters)
            .Where(p => p is not null)
            .Cast<OrbitParameters>()
            .ToList();

        return drafts
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Select(d =>
            {
                var dto = Mapper.Map(d)!;
                dto.RiskScore = _scoringService.Calculate(d.Parameters, allOrbits);
                return dto;
            });
    }
}
