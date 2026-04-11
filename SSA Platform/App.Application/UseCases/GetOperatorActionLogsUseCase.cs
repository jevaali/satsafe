using App.Application.DTO;
using App.Application.Interfaces;

namespace App.Application.UseCases;

public class GetOperatorActionLogsUseCase
{
    private readonly IUnitOfWork _uow;

    public GetOperatorActionLogsUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<OperatorActionLogDto>> ExecuteAsync(Guid operatorId, Guid? filterUserId = null)
    {
        var logs = await _uow.OperatorActionLogs.GetByOperatorIdAsync(operatorId, filterUserId);

        return logs.Select(l => new OperatorActionLogDto
        {
            UserId    = l.UserId,
            UserEmail = l.UserEmail,
            UserName  = l.UserName,
            Action    = l.Action,
            OccurredAt = l.OccurredAt
        });
    }
}
