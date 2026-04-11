using System.Globalization;
using App.Application.DTO;
using App.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels;

namespace WebApp.Controllers;

public class RegisterController : Controller
{
    private readonly RegisterUserUseCase _registerUserUseCase;
    private readonly RegisterOperatorUseCase _registerOperatorUseCase;

    public RegisterController(
        RegisterUserUseCase registerUserUseCase,
        RegisterOperatorUseCase registerOperatorUseCase)
    {
        _registerUserUseCase = registerUserUseCase;
        _registerOperatorUseCase = registerOperatorUseCase;
    }

    // GET /Register/User
    [HttpGet]
    public IActionResult User() => View(new RegisterUserViewModel());

    // POST /Register/User
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> User(RegisterUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var request = new RegisterUserRequest
        {
            Email = model.Email,
            Password = model.Password,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var (success, error) = await _registerUserUseCase.Execute(request);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Registration failed");
            return View(model);
        }

        return RedirectToPage("/Account/Login", new { area = "Identity" });
    }

    // GET /Register/Operator
    [HttpGet]
    public IActionResult Operator()
    {
        var model = new RegisterOperatorViewModel
        {
            Countries = BuildCountryList()
        };
        return View(model);
    }

    // POST /Register/Operator
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Operator(RegisterOperatorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Countries = BuildCountryList(model.OperatorCountry);
            return View(model);
        }

        var request = new RegisterOperatorRequest
        {
            Email = model.Email,
            Password = model.Password,
            FirstName = model.FirstName,
            LastName = model.LastName,
            RegisterAsOperator = true,
            OperatorName = model.OperatorName,
            OperatorCountry = model.OperatorCountry,
            OperatorType = model.OperatorType
        };

        var (success, error) = await _registerOperatorUseCase.Execute(request);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Registration failed");
            model.Countries = BuildCountryList(model.OperatorCountry);
            return View(model);
        }

        return RedirectToPage("/Account/Login", new { area = "Identity" });
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
                Value = r!.TwoLetterISORegionName,
                Text = r.DisplayName,
                Selected = string.Equals(r.TwoLetterISORegionName, selectedCode,
                    StringComparison.OrdinalIgnoreCase)
            })
            .ToList();

        items.Insert(0, new SelectListItem { Value = "", Text = "— Select country —" });
        return items;
    }
}
