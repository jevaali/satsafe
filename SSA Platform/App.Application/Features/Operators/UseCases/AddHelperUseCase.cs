using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace App.Application.Features.Operators.UseCases;

public class AddHelperUseCase
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<AppUser> _userManager;

    public AddHelperUseCase(IUnitOfWork uow, UserManager<AppUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async Task ExecuteAsync(Guid operatorId, Guid helperUserId)
    {
        var user = await _userManager.FindByIdAsync(helperUserId.ToString());

        if (user == null)
            throw new InvalidOperationException("User not found");

        var existing = await _uow.UserOperators.GetByUserAndOperatorAsync(helperUserId, operatorId);
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
            Id = Guid.NewGuid(),
            AppUserId = helperUserId,
            OperatorId = operatorId
        };

        await _uow.UserOperators.AddAsync(userOperator);
        await _uow.CommitAsync();
    }
}
