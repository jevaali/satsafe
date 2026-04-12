using System.ComponentModel.DataAnnotations;

namespace App.Application.Features.Operators.DTOs;

public class AddHelperRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
