using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class OperatorActionLogRepository : IOperatorActionLogRepository
{
    private readonly AppDbContext _context;

    public OperatorActionLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OperatorActionLog?> GetByIdAsync(Guid? id) =>
        await _context.OperatorActionLogs.FindAsync(id);

    public async Task AddAsync(OperatorActionLog entity) =>
        await _context.OperatorActionLogs.AddAsync(entity);

    public Task UpdateAsync(OperatorActionLog entity)
    {
        _context.OperatorActionLogs.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(OperatorActionLog entity)
    {
        _context.OperatorActionLogs.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<OperatorActionLog>> GetAllAsync() =>
        await _context.OperatorActionLogs.Include(l => l.User).ToListAsync();

    public async Task<IEnumerable<OperatorActionLog>> GetByOperatorIdAsync(Guid operatorId, Guid? filterUserId = null)
    {
        var query = _context.OperatorActionLogs
            .Where(l => l.OperatorId == operatorId);

        if (filterUserId.HasValue)
            query = query.Where(l => l.UserId == filterUserId.Value);

        return await query
            .OrderByDescending(l => l.OccurredAt)
            .Take(200)
            .ToListAsync();
    }
}
