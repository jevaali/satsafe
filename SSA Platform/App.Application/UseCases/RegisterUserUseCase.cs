using App.Application.DTO;
using App.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace App.Application.UseCases;

public class RegisterUserUseCase
{
    private readonly UserManager<AppUser> _userManager;

    public RegisterUserUseCase(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool Success, string? Error)> Execute(RegisterUserRequest request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return (false, "Email is already registered");

        var appUser = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(appUser, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return (false, $"Failed to create user: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(appUser);
            return (false, "Failed to assign User role");
        }

        return (true, null);
    }
}
