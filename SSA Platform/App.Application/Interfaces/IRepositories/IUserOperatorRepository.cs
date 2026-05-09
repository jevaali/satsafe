using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Interfaces.IRepositories;

public interface IUserOperatorRepository : IBaseRepository<UserOperator>
{
    Task<UserOperator?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserOperator>> GetByOperatorIdAsync(Guid operatorId);
    Task<UserOperator?> GetByUserAndOperatorAsync(Guid userId, Guid operatorId);
}
