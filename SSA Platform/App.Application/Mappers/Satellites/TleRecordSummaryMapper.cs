using App.Application.Features.Satellites.DTOs;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Mappers.Satellites;

public class TleRecordSummaryMapper : IBaseMapper<TleRecordSummaryDto, TleRecord>
{
    public TleRecordSummaryDto? Map(TleRecord? t)
    {
        if (t is null) return null;

        return new TleRecordSummaryDto
        {
            Id          = t.Id,
            Epoch       = t.Epoch,
            FetchedAt   = t.CreatedAt,
            MeanMotion  = t.MeanMotion,
            Inclination = t.Inclination
        };
    }

    public TleRecord? Map(TleRecordSummaryDto? entity) => throw new NotImplementedException();
}