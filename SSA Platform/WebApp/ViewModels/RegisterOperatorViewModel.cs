using System.ComponentModel.DataAnnotations;
using App.Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels;

public class RegisterOperatorViewModel
{
    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = default!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "Please confirm your password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = default!;

    [Required(ErrorMessage = "Operator name is required")]
    [Display(Name = "Operator Name")]
    public string OperatorName { get; set; } = default!;

    [Required(ErrorMessage = "Country is required")]
    [Display(Name = "Country")]
    public string OperatorCountry { get; set; } = default!;

    [Required(ErrorMessage = "Operator type is required")]
    [Display(Name = "Operator Type")]
    public EOperatorType? OperatorType { get; set; }

    public List<SelectListItem> Countries { get; set; } = [];
}
