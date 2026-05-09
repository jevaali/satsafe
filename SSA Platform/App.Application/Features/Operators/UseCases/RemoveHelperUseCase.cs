using App.Application.Interfaces;

namespace App.Application.Features.Operators.UseCases;

public class RemoveHelperUseCase : BaseUseCase
{
    public RemoveHelperUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task ExecuteAsync(Guid userOperatorId, Guid operatorId, Guid requestingUserId)
    {
        var userOperator = await Uow.UserOperators.FindAsync(userOperatorId)
            ?? throw new Exception("Helper entry not found.");

        if (userOperator.OperatorId != operatorId)
            throw new UnauthorizedAccessException("Cannot remove a helper from another operator.");

        if (userOperator.AppUserId == requestingUserId)
            throw new Exception("You cannot remove yourself.");

        Uow.UserOperators.Remove(userOperator);
        await Uow.CommitAsync();
    }
}
