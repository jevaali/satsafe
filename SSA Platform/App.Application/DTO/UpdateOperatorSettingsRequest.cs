using App.Domain.Entities.Enums;

namespace App.Application.DTO;

public class UpdateOperatorSettingsRequest
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public EOperatorType? OperatorType { get; set; }
}
