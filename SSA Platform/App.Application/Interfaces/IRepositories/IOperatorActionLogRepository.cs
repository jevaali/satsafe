using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface IOperatorActionLogRepository : IRepository<OperatorActionLog>
{
    Task<IEnumerable<OperatorActionLog>> GetByOperatorIdAsync(Guid operatorId, Guid? filterUserId = null);
}
