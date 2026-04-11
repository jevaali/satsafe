using App.Application.DTO;
using App.Application.Interfaces;

namespace App.Application.UseCases;

public class GetUserDraftsUseCase
{
    private readonly IUnitOfWork _uow;

    public GetUserDraftsUseCase(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<DraftSatelliteDto>> ExecuteAsync(Guid userId)
    {
        var drafts = await _uow.DraftSatellites.GetByUserIdAsync(userId);
        return drafts
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Select(SaveDraftSatelliteUseCase.ToDto);
    }
}
