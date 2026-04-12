using App.Application.Interfaces;

namespace App.Application.Features.Operators.UseCases;

public class RemoveHelperUseCase
{
    private readonly IUnitOfWork _uow;

    public RemoveHelperUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task ExecuteAsync(Guid userOperatorId, Guid operatorId, Guid requestingUserId)
    {
        var userOperator = await _uow.UserOperators.GetByIdAsync(userOperatorId)
            ?? throw new Exception("Helper entry not found.");

        if (userOperator.OperatorId != operatorId)
            throw new UnauthorizedAccessException("Cannot remove a helper from another operator.");

        if (userOperator.AppUserId == requestingUserId)
            throw new Exception("You cannot remove yourself.");

        await _uow.UserOperators.DeleteAsync(userOperator);
        await _uow.CommitAsync();
    }
}
