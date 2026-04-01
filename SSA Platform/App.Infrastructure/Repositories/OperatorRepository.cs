using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain;
using App.Domain.Entities;
using App.Infrastructure.Persistence;

namespace App.Infrastructure.Repositories;

public class OperatorRepository : IOperatorRepository
{
    private readonly AppDbContext _context;

    public OperatorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Operator?> GetByIdAsync(Guid id)
    {
        return await _context.Operators.FindAsync(id);
    }
}