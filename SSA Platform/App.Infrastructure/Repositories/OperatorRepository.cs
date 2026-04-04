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

    public async Task<Operator?> GetByIdAsync(Guid? id)
    {
        return await _context.Operators.FindAsync(id);
    }

    public async Task AddAsync(Operator entity)
    {
        await _context.Operators.AddAsync(entity);
    }

    public async Task UpdateAsync(Operator entity)
    {
        _context.Operators.Update(entity);
    }

    public async Task DeleteAsync(Operator entity)
    {
        _context.Operators.Remove(entity);
    }

    public async Task<IEnumerable<Operator>> GetAllAsync()
    {
        return await _context.Operators.ToListAsync();
    }
}