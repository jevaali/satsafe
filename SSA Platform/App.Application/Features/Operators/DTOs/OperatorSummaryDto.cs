using App.Domain.Entities.Enums;

namespace App.Application.Features.Operators.DTOs;

public class OperatorSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CountryCode { get; init; } = string.Empty;
    public EOperatorType? OperatorType { get; init; }
}
