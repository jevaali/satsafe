using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Base.Application.EF;
using Base.Contracts;
using Base.EF;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class OperatorRepository : BaseRepository<Operator, Operator, AppDbContext>, IOperatorRepository
{
    public OperatorRepository(AppDbContext context, IBaseMapper<Operator, Operator> mapper)
        : base(context, mapper)
    {
    }

    public async Task<Operator?> GetWithDetailsAsync(Guid id)
        => await RepositoryDbSet
            .Include(o => o.Satellites.Where(s => s.DeletedAt == null))
            .Include(o => o.UserOperators)
            .ThenInclude(uo => uo.AppUser)
            .FirstOrDefaultAsync(o => o.Id == id);

    public override async Task<Operator> UpdateAsync(Operator entity, Guid appUserId = default)
    {
        RepositoryDbContext.Entry(entity).State = EntityState.Modified;
        return entity;
    }
}