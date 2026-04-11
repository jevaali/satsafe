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
    private readonly GetOperatorSatellitesUseCase _getSatellitesUseCase;
    private readonly GetOperatorAlertsUseCase _getAlertsUseCase;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public OperatorHeaderViewComponent(
        GetOperatorSatellitesUseCase getSatellitesUseCase,
        GetOperatorAlertsUseCase getAlertsUseCase,
        IUserOperatorRepository userOperatorRepository)
    {
        _getSatellitesUseCase   = getSatellitesUseCase;
        _getAlertsUseCase       = getAlertsUseCase;
        _userOperatorRepository = userOperatorRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Content("");

        var satellites = await _getSatellitesUseCase.ExecuteAsync(operatorId);
        var alerts     = await _getAlertsUseCase.ExecuteAsync(operatorId);

        var vm = new OperatorHeaderViewModel
        {
            Satellites = satellites.Select(s => new SatelliteItem
            {
                Id   = s.Id,
                Name = s.Name
            }).ToList(),
            UnacknowledgedAlertCount = alerts.Count(a => !a.IsAcknowledged)
        };

        return View(vm);
    }

    private async Task<(Guid userId, Guid operatorId)> GetOperatorContextAsync()
    {
        var userIdClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return (Guid.Empty, Guid.Empty);

        if (!User.IsInRole("Operator") && !User.IsInRole("OperatorHelper"))
            return (Guid.Empty, Guid.Empty);

        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        if (userOperator == null)
            return (Guid.Empty, Guid.Empty);

        return (userId, userOperator.OperatorId);
    }
}
