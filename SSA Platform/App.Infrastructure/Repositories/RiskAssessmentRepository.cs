using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Base.Application.EF;
using Base.Contracts;
using Base.EF;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class RiskAssessmentRepository : BaseRepository<RiskAssessment, RiskAssessment, AppDbContext>, IRiskAssessmentRepository
{
    public RiskAssessmentRepository(AppDbContext context, IBaseMapper<RiskAssessment, RiskAssessment> mapper)
        : base(context, mapper)
    {
    }

    public async Task<RiskAssessment?> GetBySatelliteIdAsync(Guid satelliteId)
        => await RepositoryDbSet.FirstOrDefaultAsync(r => r.SatelliteId == satelliteId);
}