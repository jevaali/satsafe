using System.ComponentModel.DataAnnotations;
using System.Globalization;
using App.Application.Features.Operators.DTOs;
using App.Application.Features.Operators.UseCases;
using App.Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly RegisterOperatorUseCase _registerOperatorUseCase;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        RegisterOperatorUseCase registerOperatorUseCase,
        ILogger<RegisterModel> logger)
    {
        _registerOperatorUseCase = registerOperatorUseCase;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }
    public List<SelectListItem> Countries { get; set; } = [];

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        Countries = BuildCountryList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = new RegisterOperatorRequest
        {
            Email = Input.Email,
            Password = Input.Password,
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            RegisterAsOperator = Input.RegisterAsOperator,
            OperatorName = Input.OperatorName,
            OperatorCountry = Input.OperatorCountry,
            OperatorType = Input.OperatorType
        };

        var (success, error) = await _registerOperatorUseCase.Execute(request);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Registration failed");
            Countries = BuildCountryList(Input.OperatorCountry);
            return Page();
        }

        _logger.LogInformation("User registered successfully");
        
        // For now, redirect to login with success message
        return RedirectToPage("./Login", new { returnUrl });
    }

    private static List<SelectListItem> BuildCountryList(string? selectedCode = null)
    {
        var seen = new HashSet<string>();
        var items = CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .Select(c =>
            {
                try { return new RegionInfo(c.Name); }
                catch { return null; }
            })
            .Where(r => r != null && seen.Add(r.TwoLetterISORegionName))
            .OrderBy(r => r!.DisplayName)
            .Select(r => new SelectListItem
            {
                Value    = r!.TwoLetterISORegionName,
                Text     = r.DisplayName,
                Selected = string.Equals(r.TwoLetterISORegionName, selectedCode,
                                         StringComparison.OrdinalIgnoreCase)
            })
            .ToList();

        items.Insert(0, new SelectListItem { Value = "", Text = "— Select country —" });
        return items;
    }

    public class InputModel
    {
        // Validation attributes removed - now handled by RegisterOperatorRequest DTO
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = default!;

        [Display(Name = "Last Name")]
        public string LastName { get; set; } = default!;

        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = default!;

        [Display(Name = "Register as Operator")]
        public bool RegisterAsOperator { get; set; }

        [Display(Name = "Operator Name")]
        public string? OperatorName { get; set; }

        [Display(Name = "Country")]
        public string? OperatorCountry { get; set; }

        [Display(Name = "Operator Type")]
        public EOperatorType? OperatorType { get; set; }
    }
}