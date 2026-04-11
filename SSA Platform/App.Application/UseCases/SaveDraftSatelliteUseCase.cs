using App.Application.DTO;
using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.ValueObjects;

namespace App.Application.UseCases;

public class SaveDraftSatelliteUseCase
{
    private readonly IUnitOfWork _uow;

    public SaveDraftSatelliteUseCase(IUnitOfWork uow) => _uow = uow;

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
            var existing = await _uow.DraftSatellites.GetByIdAndUserAsync(request.Id.Value, userId)
                ?? throw new InvalidOperationException("Draft not found.");
            existing.Update(request.Name, request.Notes, parameters);
            draft = existing;
        }
        else
        {
            draft = DraftSatellite.Create(userId, request.Name, request.Notes, parameters);
            await _uow.DraftSatellites.AddAsync(draft);
        }

        await _uow.CommitAsync();

        return ToDto(draft);
    }

    internal static DraftSatelliteDto ToDto(DraftSatellite d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Notes = d.Notes,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt,
        SemiMajorAxis = d.Parameters.SemiMajorAxis,
        Eccentricity = d.Parameters.Eccentricity,
        Inclination = d.Parameters.Inclination,
        RAAN = d.Parameters.RAAN,
        ArgumentOfPerigee = d.Parameters.ArgumentOfPerigee,
        MeanAnomaly = d.Parameters.MeanAnomaly
    };
}
