using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Base.Application.EF;
using Base.EF;
using Base.Contracts;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;
public class AlertRepository : BaseRepository<Alert, Alert, AppDbContext>, IAlertRepository
{
    public AlertRepository(AppDbContext context, IBaseMapper<Alert, Alert> mapper) 
        : base(context, mapper)
    {
    }

    public async Task<IEnumerable<Alert>> GetByConjunctionIdsAsync(List<Guid> conjunctionIds)
    {
        return await RepositoryDbSet
            .Include(a => a.User)
            .Include(a => a.ConjunctionEvent)
            .Where(a => conjunctionIds.Contains(a.ConjunctionEventId))
            .ToListAsync();
    }

    public Task UpdateMessageAsync(Alert entity)
    {
        RepositoryDbContext.Entry(entity).Property(e => e.Message).IsModified = true;
        return Task.CompletedTask;
    }

    public Task UpdateAcknowledgedAsync(Alert entity)
    {
        RepositoryDbContext.Entry(entity).Property(e => e.IsAcknowledged).IsModified = true;
        return Task.CompletedTask;
    }
}