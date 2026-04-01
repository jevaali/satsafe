using App.Application.DTO;
using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace App.Application.UseCases;

public class RegisterOperatorUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _uow;

    public RegisterOperatorUseCase(UserManager<AppUser> userManager, IUnitOfWork uow)
    {
        _userManager = userManager;
        _uow = uow;
    }

    public async Task<(bool Success, string? Error)> Execute(RegisterOperatorRequest request)
    {
        // Database-dependent validations only
        
        // Check email uniqueness (requires database access)
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return (false, "Email is already registered");
        
        // Conditional operator field validation (depends on request data)
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
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
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
                    Id = Guid.NewGuid(),
                    Name = request.OperatorName!,
                    Country = request.OperatorCountry!,
                    OperatorType = request.OperatorType!.Value,
                    CreatedBy = appUser.Id.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.Operators.AddAsync(operatorEntity);

                var userOperator = new UserOperator
                {
                    Id = Guid.NewGuid(),
                    AppUserId = appUser.Id,
                    OperatorId = operatorEntity.Id,
                };

                await _uow.UserOperators.AddAsync(userOperator);
            }

            await _uow.CommitAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            await _userManager.DeleteAsync(appUser);
            return (false, $"Registration failed: {ex.Message}");
        }
    }
}