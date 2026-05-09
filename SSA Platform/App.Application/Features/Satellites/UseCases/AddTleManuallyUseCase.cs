using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.Features.Satellites.UseCases;

public class AddTleManuallyUseCase : BaseUseCase
{
    public AddTleManuallyUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task ExecuteAsync(Guid satelliteId, string line1, string line2, string addedBy, Guid? requiredOperatorId = null)
    {
        var satellite = await Uow.Satellites.FindAsync(satelliteId)
            ?? throw new Exception("Satellite not found");

        if (requiredOperatorId.HasValue && satellite.OperatorId != requiredOperatorId)
            throw new Exception("Access denied: satellite does not belong to your operator");

        var dto = TleParser.Parse(line1, line2)
            ?? throw new Exception("Invalid TLE format. Make sure Line 1 starts with '1 ' and Line 2 starts with '2 ', and each is exactly 69 characters.");

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
            RawLine1          = line1,
            RawLine2          = line2,
            CreatedBy         = addedBy,
            CreatedAt         = DateTime.UtcNow
        };

        await Uow.Satellites.AddTleRecordAsync(record);
        await Uow.CommitAsync();
    }
}
