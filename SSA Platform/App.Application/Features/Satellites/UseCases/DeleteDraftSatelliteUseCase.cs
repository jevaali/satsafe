using App.Application.Interfaces;

namespace App.Application.Features.Satellites.UseCases;

public class DeleteDraftSatelliteUseCase
{
    private readonly IUnitOfWork _uow;

    public DeleteDraftSatelliteUseCase(IUnitOfWork uow) => _uow = uow;

    /// <returns>false if not found or not owned by this user</returns>
    public async Task<bool> ExecuteAsync(Guid userId, Guid draftId)
    {
        var draft = await _uow.DraftSatellites.GetByIdAndUserAsync(draftId, userId);
        if (draft == null) return false;

        await _uow.DraftSatellites.DeleteAsync(draft);
        await _uow.CommitAsync();
        return true;
    }
}
