using App.Application.Interfaces;

namespace App.Application.Features.Satellites.UseCases;

public class DeleteDraftSatelliteUseCase : BaseUseCase
{
    public DeleteDraftSatelliteUseCase(IUnitOfWork uow) : base(uow) { }

    /// <returns>false if not found or not owned by this user</returns>
    public async Task<bool> ExecuteAsync(Guid userId, Guid draftId)
    {
        var draft = await Uow.DraftSatellites.GetByIdAndUserAsync(draftId, userId);
        if (draft == null) return false;

        Uow.DraftSatellites.Remove(draft);
        await Uow.CommitAsync();
        return true;
    }
}
