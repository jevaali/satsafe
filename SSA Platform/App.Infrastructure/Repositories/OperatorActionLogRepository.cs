using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Base.Application.EF;
using Base.Contracts;
using Base.EF;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class OperatorActionLogRepository : BaseRepository<OperatorActionLog, OperatorActionLog, AppDbContext>, IOperatorActionLogRepository
{
    public OperatorActionLogRepository(AppDbContext context, IBaseMapper<OperatorActionLog, OperatorActionLog> mapper)
        : base(context, mapper)
    {
    }
    
    public async Task<IEnumerable<OperatorActionLog>> GetByOperatorIdAsync(Guid operatorId, Guid? filterUserId = null)
    {
        var query = RepositoryDbSet
            .Where(l => l.OperatorId == operatorId);

        if (filterUserId.HasValue)
            query = query.Where(l => l.UserId == filterUserId.Value);

        return await query
            .OrderByDescending(l => l.OccurredAt)
            .Take(200)
            .ToListAsync();
    }
}