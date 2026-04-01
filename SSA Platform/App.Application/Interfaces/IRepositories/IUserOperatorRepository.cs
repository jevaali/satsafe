using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface IUserOperatorRepository : IRepository<UserOperator>
{
    Task<UserOperator?> GetByUserIdAsync(Guid userId);
}
