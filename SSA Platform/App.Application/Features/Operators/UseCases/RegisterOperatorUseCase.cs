using App.Application.Features.Operators.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace App.Application.Features.Operators.UseCases;

public class RegisterOperatorUseCase : BaseUseCase
{
    private readonly UserManager<AppUser> _userManager;

    public RegisterOperatorUseCase(IUnitOfWork uow, UserManager<AppUser> userManager) : base(uow)
    {
        _userManager = userManager;
    }

    public async Task<(bool Success, string? Error)> Execute(RegisterOperatorRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return (false, "Email is already registered");

        if (request.RegisterAsOperator)
        {
            if (string.IsNullOrWhiteSpace(request.OperatorName))
                return (false, "Operator name is required");

            if (string.IsNullOrWhiteSpace(request.OperatorCountry))
                return (false, "Operator country is required");

            if (!request.OperatorType.HasValue)
                return (false, "Operator type is required");
        }

        var appUser = new AppUser
        {
            UserName  = request.Email,
            Email     = request.Email,
            FirstName = request.FirstName,
            LastName  = request.LastName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createUserResult = await _userManager.CreateAsync(appUser, request.Password);
        if (!createUserResult.Succeeded)
        {
            var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
            return (false, $"Failed to create user: {errors}");
        }

        try
        {
            if (request.RegisterAsOperator)
            {
                var roleResult = await _userManager.AddToRoleAsync(appUser, "Operator");
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(appUser);
                    return (false, "Failed to assign Operator role");
                }

                var operatorEntity = new Operator
                {
                    Id           = Guid.NewGuid(),
                    Name         = request.OperatorName!,
                    CountryCode  = request.OperatorCountry!,
                    OperatorType = request.OperatorType!.Value,
                    CreatedBy    = appUser.Id.ToString(),
                    CreatedAt    = DateTime.UtcNow
                };

                Uow.Operators.Add(operatorEntity);

                var userOperator = new UserOperator
                {
                    Id         = Guid.NewGuid(),
                    AppUserId  = appUser.Id,
                    OperatorId = operatorEntity.Id,
                };

                Uow.UserOperators.Add(userOperator);
            }

            await Uow.CommitAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            await _userManager.DeleteAsync(appUser);
            return (false, $"Registration failed: {ex.Message}");
        }
    }
}
