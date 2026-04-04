using App.Application.DTO;
using App.Application.Interfaces;

namespace App.Application.UseCases;

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

        op.Name = request.Name;
        op.Country = request.Country;
        op.OperatorType = request.OperatorType;

        await _uow.Operators.UpdateAsync(op);
        await _uow.CommitAsync();
    }
}
