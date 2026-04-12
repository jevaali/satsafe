using System.ComponentModel.DataAnnotations;
using App.Domain.Entities.Enums;

namespace App.Application.Features.Operators.DTOs;

public class AddOperatorRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name must not exceed 200 characters")]
    [Display(Name = "Name")]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "Country is required")]
    [StringLength(100, ErrorMessage = "Country must not exceed 100 characters")]
    [Display(Name = "Country")]
    public string Country { get; set; } = default!;

    [Required(ErrorMessage = "Operator type is required")]
    [Display(Name = "Operator Type")]
    public EOperatorType OperatorType { get; set; }
}
