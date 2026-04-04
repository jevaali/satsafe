using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class UserOperatorRepository : IUserOperatorRepository
{
    private readonly AppDbContext _context;

    public UserOperatorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserOperator?> GetByIdAsync(Guid? id)
    {
        return await _context.UserOperators.FindAsync(id);
    }

    public async Task AddAsync(UserOperator entity)
    {
        await _context.UserOperators.AddAsync(entity);
    }

    public async Task UpdateAsync(UserOperator entity)
    {
        _context.UserOperators.Update(entity);
    }

    public async Task DeleteAsync(UserOperator entity)
    {
        _context.UserOperators.Remove(entity);
    }

    public async Task<IEnumerable<UserOperator>> GetAllAsync()
    {
        return await _context.UserOperators
            .Include(u => u.AppUser)
            .Include(u => u.Operator)
            .ToListAsync();
    }

    public async Task<UserOperator?> GetByUserIdAsync(Guid userId) =>
        await _context.UserOperators.FirstOrDefaultAsync(u => u.AppUserId == userId);

    public async Task<IEnumerable<UserOperator>> GetByOperatorIdAsync(Guid operatorId) =>
        await _context.UserOperators
            .Include(u => u.AppUser)
            .Where(u => u.OperatorId == operatorId)
            .ToListAsync();

    public async Task<UserOperator?> GetByUserAndOperatorAsync(Guid userId, Guid operatorId) =>
        await _context.UserOperators
            .FirstOrDefaultAsync(u => u.AppUserId == userId && u.OperatorId == operatorId);
}