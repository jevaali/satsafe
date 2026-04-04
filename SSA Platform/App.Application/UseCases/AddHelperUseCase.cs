using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.UseCases;

public class AddHelperUseCase
{
    private readonly IUnitOfWork _uow;

    public AddHelperUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task ExecuteAsync(Guid operatorId, Guid helperUserId)
    {
        var existing = await _uow.UserOperators.GetByUserAndOperatorAsync(helperUserId, operatorId);
        if (existing != null)
            throw new Exception("This user already has access to this operator.");

        var userOperator = new UserOperator
        {
            Id = Guid.NewGuid(),
            AppUserId = helperUserId,
            OperatorId = operatorId
        };

        await _uow.UserOperators.AddAsync(userOperator);
        await _uow.CommitAsync();
    }
}
