using App.Application.Interfaces;

namespace App.Application.Features.Operators.UseCases;

public class GetUserContextUseCase
{
    private readonly IUnitOfWork _uow;

    public GetUserContextUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    /// <summary>
    /// Resolves the operatorId for the given userId.
    /// Returns (userId, operatorId), or (Empty, Empty) if no operator is linked.
    /// </summary>
    public async Task<(Guid UserId, Guid OperatorId)> ExecuteAsync(string? userIdClaim)
    {
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return (Guid.Empty, Guid.Empty);

        var userOperator = await _uow.UserOperators.GetByUserIdAsync(userId);
        return userOperator == null
            ? (Guid.Empty, Guid.Empty)
            : (userId, userOperator.OperatorId);
    }
}
