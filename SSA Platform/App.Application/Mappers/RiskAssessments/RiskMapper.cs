using App.Application.Features.Satellites.DTOs;
using Base.Contracts;

namespace App.Application.Mappers.RiskAssessments;

public class RiskMapper : IBaseMapper<RiskAssessmentDto, Domain.Entities.RiskAssessment>
{
    public RiskAssessmentDto? Map(Domain.Entities.RiskAssessment? r)
    {
        if (r is null) return null;

        return new RiskAssessmentDto
        {
            RiskScore    = r.RiskScore,
            RiskLevel    = r.RiskLevel,
            ModelVersion = r.ModelVersion,
            CalculatedAt = r.CalculatedAt
        };
    }

    public Domain.Entities.RiskAssessment? Map(RiskAssessmentDto? entity) => throw new NotImplementedException();
}