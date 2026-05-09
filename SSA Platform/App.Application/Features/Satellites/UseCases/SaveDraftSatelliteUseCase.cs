using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.ValueObjects;
using Base.Contracts;

namespace App.Application.Features.Satellites.UseCases;

public class SaveDraftSatelliteUseCase : BaseUseCase<DraftSatelliteDto, DraftSatellite>
{
    public SaveDraftSatelliteUseCase(IUnitOfWork uow, IBaseMapper<DraftSatelliteDto, DraftSatellite> mapper)
        : base(uow, mapper) { }

    public async Task<DraftSatelliteDto> ExecuteAsync(Guid userId, SaveDraftSatelliteRequest request)
    {
        var parameters = new OrbitParameters(
            request.SemiMajorAxis,
            request.Eccentricity,
            request.Inclination,
            request.RAAN,
            request.ArgumentOfPerigee,
            request.MeanAnomaly);

        DraftSatellite draft;

        if (request.Id.HasValue)
        {
            var existing = await Uow.DraftSatellites.GetByIdAndUserAsync(request.Id.Value, userId)
                           ?? throw new InvalidOperationException("Draft not found.");
            existing.Update(request.Name, request.Notes, parameters);
            draft = existing;
        }
        else
        {
            draft = DraftSatellite.Create(userId, request.Name, request.Notes, parameters);
            Uow.DraftSatellites.Add(draft);
        }

        await Uow.CommitAsync();

        return Mapper.Map(draft)!;
    }
}
