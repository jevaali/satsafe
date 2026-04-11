using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface IRiskAssessmentRepository
{
    Task<RiskAssessment?> GetBySatelliteIdAsync(Guid satelliteId);
    Task AddAsync(RiskAssessment assessment);
    Task UpdateAsync(RiskAssessment assessment);
}
