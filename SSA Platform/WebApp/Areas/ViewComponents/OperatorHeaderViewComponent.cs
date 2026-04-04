using App.Application.UseCases;
using App.Application.Interfaces.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApp.Models;

[Area("Operator")]
[Authorize]
public class OperatorHeaderViewComponent : ViewComponent
{
    private readonly GetOperatorDashboardUseCase _dashboardUseCase;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public OperatorHeaderViewComponent(
        GetOperatorDashboardUseCase dashboardUseCase,
        IUserOperatorRepository userOperatorRepository)
    {
        _dashboardUseCase = dashboardUseCase;
        _userOperatorRepository = userOperatorRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Content(""); // return empty if no operator context

        var satellites = await _dashboardUseCase.GetSatellitesAsync(operatorId);

        var vm = new OperatorHeaderViewModel
        {
            Satellites = satellites.Select(s => new SatelliteItem
            {
                Id = s.Id,
                Name = s.Name
            }).ToList()
        };

        return View(vm); 
    }

    private async Task<(Guid userId, Guid operatorId)> GetOperatorContextAsync()
    {
        var userIdClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return (Guid.Empty, Guid.Empty);

        if (!User.IsInRole("Operator"))
            return (Guid.Empty, Guid.Empty);

        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        if (userOperator == null)
            return (Guid.Empty, Guid.Empty);

        return (userId, userOperator.OperatorId);
    }
}