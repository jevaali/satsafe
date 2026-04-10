using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _context;

    public AlertRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Alert?> GetByIdAsync(Guid? id)
    {
        return await _context.Alerts
            .Include(a => a.User)
            .Include(a => a.ConjunctionEvent)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAsync(Alert entity)
    {
        _context.Alerts.Add(entity);
    }

    public async Task UpdateAsync(Alert entity)
    {
        _context.Alerts.Update(entity);
    }

    public async Task DeleteAsync(Alert entity)
    {
        _context.Alerts.Remove(entity);
    }

    public async Task<IEnumerable<Alert>> GetAllAsync()
    {
        return await _context.Alerts
            .Include(a => a.User)
            .Include(a => a.ConjunctionEvent)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alert>> GetByConjunctionIdsAsync(List<Guid> conjunctionIds)
    {
        return await _context.Alerts
            .Include(a => a.User)
            .Include(a => a.ConjunctionEvent)
            .Where(a => conjunctionIds.Contains(a.ConjunctionEventId))
            .ToListAsync();
    }

    public Task UpdateMessageAsync(Alert entity)
    {
        _context.Entry(entity).Property(e => e.Message).IsModified = true;
        return Task.CompletedTask;
    }
}
