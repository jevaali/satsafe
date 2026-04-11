using App.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using WebApp.ViewModels;

namespace WebApp.UseCases;

public class LoginUseCase
{
    private readonly SignInManager<AppUser> _signInManager;

    public LoginUseCase(SignInManager<AppUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<(bool Success, bool IsLockedOut, string? Error)> Execute(LoginViewModel model)
    {
        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
            return (true, false, null);

        if (result.IsLockedOut)
            return (false, true, "This account has been locked out. Please try again later.");

        return (false, false, "Invalid email or password.");
    }
}
