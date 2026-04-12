using System.ComponentModel.DataAnnotations;
using App.Domain.Entities.Enums;

namespace App.Application.Features.Operators.DTOs;

public class RegisterOperatorRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = default!;

    // Operator fields (optional - only if registering as operator)
    [Display(Name = "Register as Operator")]
    public bool RegisterAsOperator { get; set; }

    [Display(Name = "Operator Name")]
    public string? OperatorName { get; set; }

    [Display(Name = "Country")]
    public string? OperatorCountry { get; set; }

    [Display(Name = "Operator Type")]
    public EOperatorType? OperatorType { get; set; }
}
