using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class RiskAssessmentRepository : IRiskAssessmentRepository
{
    private readonly AppDbContext _context;

    public RiskAssessmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RiskAssessment?> GetBySatelliteIdAsync(Guid satelliteId)
    {
        return await _context.RiskAssessments
            .FirstOrDefaultAsync(r => r.SatelliteId == satelliteId);
    }

    public async Task AddAsync(RiskAssessment assessment)
    {
        await _context.RiskAssessments.AddAsync(assessment);
    }

    public Task UpdateAsync(RiskAssessment assessment)
    {
        _context.RiskAssessments.Update(assessment);
        return Task.CompletedTask;
    }
}
