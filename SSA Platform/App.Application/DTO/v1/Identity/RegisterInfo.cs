using App.Domain.Entities.Enums;

namespace App.Application.DTO.v1.Identity;

public class RegisterInfo
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Firstname { get; set; } = default!;
    public string Lastname { get; set; } = default!;

    // Operator registration (optional)
    public bool RegisterAsOperator { get; set; }
    public string? OperatorName { get; set; }
    public string? OperatorCountry { get; set; }
    public EOperatorType? OperatorType { get; set; }
}
