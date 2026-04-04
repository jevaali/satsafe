using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface IUserOperatorRepository : IRepository<UserOperator>
{
    Task<UserOperator?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserOperator>> GetByOperatorIdAsync(Guid operatorId);
    Task<UserOperator?> GetByUserAndOperatorAsync(Guid userId, Guid operatorId);
}
