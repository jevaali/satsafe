using App.Application.Features.Dashboard.DTOs;
using App.Application.Interfaces;

namespace App.Application.Features.Dashboard.UseCases;

public class GetOperatorActionLogsUseCase : BaseUseCase
{
    public GetOperatorActionLogsUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task<IEnumerable<OperatorActionLogDto>> ExecuteAsync(Guid operatorId, Guid? filterUserId = null)
    {
        var logs = await Uow.OperatorActionLogs.GetByOperatorIdAsync(operatorId, filterUserId);

        return logs.Select(l => new OperatorActionLogDto
        {
            UserId     = l.UserId,
            UserEmail  = l.UserEmail,
            UserName   = l.UserName,
            Action     = l.Action,
            OccurredAt = l.OccurredAt
        });
    }
}
