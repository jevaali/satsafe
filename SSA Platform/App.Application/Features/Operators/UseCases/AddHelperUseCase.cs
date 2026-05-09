using App.Application.Features.Operators.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace App.Application.Features.Operators.UseCases;

public class AddHelperUseCase : BaseUseCase
{
    private readonly UserManager<AppUser> _userManager;

    public AddHelperUseCase(IUnitOfWork uow, UserManager<AppUser> userManager) : base(uow)
    {
        _userManager = userManager;
    }

    public async Task ExecuteAsync(Guid operatorId, AddHelperRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException($"No user found with email '{request.Email}'.");

        var existing = await Uow.UserOperators.GetByUserAndOperatorAsync(user.Id, operatorId);
        if (existing != null)
            throw new Exception("This user already has access to this operator.");

        var isInRole = await _userManager.IsInRoleAsync(user, "OperatorHelper");

        if (!isInRole)
        {
            await _userManager.RemoveFromRoleAsync(user, "User");
            var result = await _userManager.AddToRoleAsync(user, "OperatorHelper");

            if (!result.Succeeded)
                throw new Exception("Failed to assign role.");
        }

        var userOperator = new UserOperator
        {
            Id         = Guid.NewGuid(),
            AppUserId  = user.Id,
            OperatorId = operatorId
        };

        Uow.UserOperators.Add(userOperator);
        await Uow.CommitAsync();
    }
}
