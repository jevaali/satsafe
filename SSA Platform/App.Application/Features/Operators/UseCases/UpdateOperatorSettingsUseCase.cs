using App.Application.Features.Operators.DTOs;
using App.Application.Interfaces;

namespace App.Application.Features.Operators.UseCases;

public class UpdateOperatorSettingsUseCase
{
    private readonly IUnitOfWork _uow;

    public UpdateOperatorSettingsUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task ExecuteAsync(Guid operatorId, UpdateOperatorSettingsRequest request)
    {
        var op = await _uow.Operators.GetByIdAsync(operatorId)
            ?? throw new Exception("Operator not found");
        
        op.Name?.SetTranslation(request.Name);
        op.CountryCode = request.Country;
        op.OperatorType = request.OperatorType;

        await _uow.Operators.UpdateAsync(op);
        await _uow.CommitAsync();
    }
}
