using System.ComponentModel.DataAnnotations;
using App.Domain.Entities.Enums;

namespace App.Application.Features.Operators.DTOs;

public class UpdateOperatorSettingsRequest
{
    [Required, MaxLength(124)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(124)]
    public string Country { get; set; } = string.Empty;

    public EOperatorType? OperatorType { get; set; }
}
