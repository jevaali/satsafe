using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Interfaces.IRepositories;

public interface IOperatorActionLogRepository : IBaseRepository<OperatorActionLog>
{
    Task<IEnumerable<OperatorActionLog>> GetByOperatorIdAsync(Guid operatorId, Guid? filterUserId = null);
}
