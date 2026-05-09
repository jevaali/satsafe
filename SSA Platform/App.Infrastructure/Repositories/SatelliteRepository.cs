using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using App.Infrastructure.Persistence;
using Base.Application.EF;
using Base.Contracts;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class SatelliteRepository : BaseRepository<Satellite, Satellite, AppDbContext>, ISatelliteRepository
{
    public SatelliteRepository(AppDbContext context, IBaseMapper<Satellite, Satellite> mapper)
        : base(context, mapper)
    {
    }
    public override async Task<Satellite?> FindAsync(Guid id, Guid appUserId = default)
        => await RepositoryDbSet
            .Include(s => s.Operator)
            .Include(s => s.Orbit)
            .Include(s => s.TleRecords)
            .Include(s => s.RiskAssessment)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<IEnumerable<Satellite>> GetByOperatorIdAsync(Guid operatorId)
        => await RepositoryDbSet
            .Include(s => s.Operator)
            .Include(s => s.Orbit)
            .Include(s => s.TleRecords)
            .Include(s => s.RiskAssessment)
            .Where(s => s.OperatorId == operatorId && s.Status != ESatelliteStatus.Deleted)
            .ToListAsync();

    public Task<int> GetCountAsync()
        => RepositoryDbSet.CountAsync();

    public Task<int> GetActiveCountAsync()
        => RepositoryDbSet.CountAsync(s => s.Status == ESatelliteStatus.Active);

    public Task<List<Satellite>> GetTopAsync(int count)
        => RepositoryDbSet
            .Include(s => s.Operator)
            .Take(count)
            .ToListAsync();

    public async Task AddTleRecordAsync(TleRecord record)
        => await RepositoryDbContext.Set<TleRecord>().AddAsync(record);

    public override async Task<Satellite> UpdateAsync(Satellite entity, Guid appUserId = default)
    {
        RepositoryDbContext.Entry(entity).State = EntityState.Modified;
        return entity;
    }
}