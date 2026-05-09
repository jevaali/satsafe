using App.Application.Features.Operators.DTOs;
using App.Application.Interfaces;

namespace App.Application.Features.Operators.UseCases;

public class UpdateOperatorSettingsUseCase : BaseUseCase
{
    public UpdateOperatorSettingsUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task ExecuteAsync(Guid operatorId, UpdateOperatorSettingsRequest request)
    {
        var op = await Uow.Operators.FindAsync(operatorId)
            ?? throw new Exception("Operator not found");

        op.Name?.SetTranslation(request.Name);
        op.CountryCode  = request.Country;
        op.OperatorType = request.OperatorType;

        await Uow.Operators.UpdateAsync(op);
        await Uow.CommitAsync();
    }
}
