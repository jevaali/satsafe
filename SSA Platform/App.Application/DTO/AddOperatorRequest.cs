using App.Domain.Entities.Enums;

namespace App.Application.DTO;

public class AddOperatorRequest
{
    public string Name { get; set; } = default!;
    public string Country { get; set; } = default!;
    public EOperatorType OperatorType { get; set; }
}