using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Interfaces.IRepositories;

public interface IRiskAssessmentRepository: IBaseRepository<RiskAssessment>
{
    Task<RiskAssessment?> GetBySatelliteIdAsync(Guid satelliteId);

}
