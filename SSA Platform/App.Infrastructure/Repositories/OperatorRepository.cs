using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class OperatorRepository : IOperatorRepository
{
    private readonly AppDbContext _context;

    public OperatorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Operator?> GetByIdAsync(Guid? id) =>
        await _context.Operators.FindAsync(id);

    public async Task<Operator?> GetWithDetailsAsync(Guid id) =>
        await _context.Operators
            .Include(o => o.Satellites.Where(s => s.DeletedAt == null))
            .Include(o => o.UserOperators)
                .ThenInclude(uo => uo.AppUser)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task AddAsync(Operator entity) =>
        await _context.Operators.AddAsync(entity);

    public Task UpdateAsync(Operator entity)
    {
        _context.Operators.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Operator entity)
    {
        _context.Operators.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Operator>> GetAllAsync() =>
        await _context.Operators.ToListAsync();
}