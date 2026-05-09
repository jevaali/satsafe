using App.Application.Features.Operators.DTOs;
using Base.Contracts;

namespace App.Application.Mappers.Operator;

public class OperatorSummaryMapper : IBaseMapper<OperatorSummaryDto, Domain.Entities.Operator>
{
    public OperatorSummaryDto? Map(Domain.Entities.Operator? op)
    {
        if (op is null) return null;

        return new OperatorSummaryDto
        {
            Id           = op.Id,
            Name         = op.Name.ToString(),
            CountryCode  = op.CountryCode,
            OperatorType = op.OperatorType
        };
    }

    public Domain.Entities.Operator? Map(OperatorSummaryDto? entity) => throw new NotImplementedException();
}