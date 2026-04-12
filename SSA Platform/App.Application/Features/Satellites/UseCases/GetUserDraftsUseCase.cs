using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;
using App.Domain.Services;
using App.Domain.ValueObjects;

namespace App.Application.Features.Satellites.UseCases;

public class GetUserDraftsUseCase
{
    private readonly IUnitOfWork _uow;
    private readonly SatelliteRiskScoringService _scoringService;

    public GetUserDraftsUseCase(IUnitOfWork uow, SatelliteRiskScoringService scoringService)
    {
        _uow = uow;
        _scoringService = scoringService;
    }

    public async Task<IEnumerable<DraftSatelliteDto>> ExecuteAsync(Guid userId)
    {
        var drafts = await _uow.DraftSatellites.GetByUserIdAsync(userId);

        var allOrbits = (await _uow.SpaceObjects.GetAllWithOrbitsAsync())
            .Select(o => o.Orbit?.Parameters)
            .Where(p => p is not null)
            .Cast<OrbitParameters>()
            .ToList();

        return drafts
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Select(d =>
            {
                var dto = SaveDraftSatelliteUseCase.ToDto(d);
                dto.RiskScore = _scoringService.Calculate(d.Parameters, allOrbits);
                return dto;
            });
    }
}
