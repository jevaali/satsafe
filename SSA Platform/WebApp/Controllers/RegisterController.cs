using System.Globalization;
using App.Application.Features.Operators.DTOs;
using App.Application.Features.Operators.UseCases;
using App.Application.Features.Users.DTOs;
using App.Application.Features.Users.UseCases;
using App.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels;

namespace WebApp.Controllers;

public class RegisterController : Controller
{
    private readonly RegisterUserUseCase _registerUserUseCase;
    private readonly RegisterOperatorUseCase _registerOperatorUseCase;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public RegisterController(
        RegisterUserUseCase registerUserUseCase,
        RegisterOperatorUseCase registerOperatorUseCase,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager)
    {
        _registerUserUseCase = registerUserUseCase;
        _registerOperatorUseCase = registerOperatorUseCase;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET /Register/User
    [HttpGet]
    public IActionResult User() => View(new RegisterUserViewModel());

    // POST /Register/User
    [HttpPost]
    [ValidateAntiForgeryToken]
    // POST /Register/User
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

        var user = await _userManager.FindByEmailAsync(model.Email);
        await _signInManager.SignInAsync(user!, isPersistent: false);
        return RedirectToAction("Index", "Home", new { area = "User" });
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

        var user = await _userManager.FindByEmailAsync(model.Email);
        await _signInManager.SignInAsync(user!, isPersistent: false);
        return RedirectToAction("Index", "Home", new { area = "Operator" });
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