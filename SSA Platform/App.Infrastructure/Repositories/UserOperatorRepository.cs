using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Base.Application.EF;
using Base.Contracts;
using Base.EF;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class UserOperatorRepository : BaseRepository<UserOperator, UserOperator, AppDbContext>, IUserOperatorRepository
{
    public UserOperatorRepository(AppDbContext context, IBaseMapper<UserOperator, UserOperator> mapper)
        : base(context, mapper)
    {
    }
    
    public async Task<UserOperator?> GetByUserIdAsync(Guid userId)
        => await RepositoryDbSet.FirstOrDefaultAsync(u => u.AppUserId == userId);

    public async Task<IEnumerable<UserOperator>> GetByOperatorIdAsync(Guid operatorId)
        => await RepositoryDbSet
            .Include(u => u.AppUser)
            .Where(u => u.OperatorId == operatorId)
            .ToListAsync();

    public async Task<UserOperator?> GetByUserAndOperatorAsync(Guid userId, Guid operatorId)
        => await RepositoryDbSet
            .FirstOrDefaultAsync(u => u.AppUserId == userId && u.OperatorId == operatorId);
}