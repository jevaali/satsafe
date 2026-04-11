using App.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.UseCases;
using WebApp.ViewModels;

namespace WebApp.Controllers;

public class LoginController : Controller
{
    private readonly LoginUseCase _loginUseCase;
    private readonly SignInManager<AppUser> _signInManager;

    public LoginController(LoginUseCase loginUseCase, SignInManager<AppUser> signInManager)
    {
        _loginUseCase = loginUseCase;
        _signInManager = signInManager;
    }

    // GET /Login
    [HttpGet]
    public IActionResult Index(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User))
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    // POST /Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var (success, isLockedOut, error) = await _loginUseCase.Execute(model);

        if (isLockedOut)
            return View("Lockout");

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Login failed");
            return View(model);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    // POST /Login/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
