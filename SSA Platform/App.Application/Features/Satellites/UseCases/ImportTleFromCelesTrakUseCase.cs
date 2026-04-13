using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.Features.Satellites.UseCases;

public class ImportTleFromCelesTrakUseCase
{
    private readonly IUnitOfWork _uow;
    private readonly ICelesTrakService _celestrak;

    public ImportTleFromCelesTrakUseCase(IUnitOfWork uow, ICelesTrakService celestrak)
    {
        _uow       = uow;
        _celestrak = celestrak;
    }

    public async Task<Guid> ExecuteAsync(Guid satelliteId, string fetchedBy, Guid? requiredOperatorId = null)
    {
        var satellite = await _uow.Satellites.GetByIdAsync(satelliteId)
            ?? throw new Exception("Satellite not found");

        if (requiredOperatorId.HasValue && satellite.OperatorId != requiredOperatorId)
            throw new Exception("Access denied: satellite does not belong to your operator");

        if (satellite.NoradId == 0)
            throw new Exception("Satellite has no NORAD ID assigned");

        var dto = await _celestrak.FetchTleByNoradIdAsync(satellite.NoradId)
            ?? throw new Exception($"CelesTrak returned no data for NORAD {satellite.NoradId}. Check the NORAD ID is correct.");

        var record = new TleRecord
        {
            Id                = Guid.NewGuid(),
            SpaceObjectId     = satellite.Id,
            SpaceObject       = satellite,
            Epoch             = dto.Epoch,
            Inclination       = dto.Inclination,
            RAAN              = dto.RAAN,
            Eccentricity      = dto.Eccentricity,
            ArgumentOfPerigee = dto.ArgumentOfPerigee,
            MeanAnomaly       = dto.MeanAnomaly,
            MeanMotion        = dto.MeanMotion,
            BSTar             = dto.BSTar,
            FirstDerivative   = dto.FirstDerivative,
            SecondDerivative  = dto.SecondDerivative,
            RevolutionNumber  = dto.RevolutionNumber,
            RawLine1          = dto.Line1,
            RawLine2          = dto.Line2,
            CreatedBy         = fetchedBy,
            CreatedAt         = DateTime.UtcNow
        };
        
        await _uow.Satellites.AddTleRecordAsync(record);
        await _uow.CommitAsync();

        return record.Id;
    }
}
