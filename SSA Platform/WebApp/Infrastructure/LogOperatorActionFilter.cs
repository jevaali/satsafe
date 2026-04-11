using System.Security.Claims;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApp.Infrastructure;

public class LogOperatorActionFilter : IAsyncActionFilter
{
    private readonly AppDbContext _context;
    private readonly UserOperatorRepository _userOperatorRepo;

    public LogOperatorActionFilter(AppDbContext context, UserOperatorRepository userOperatorRepo)
    {
        _context = context;
        _userOperatorRepo = userOperatorRepo;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();

        // Only log POST requests in the Operator area
        if (!context.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            return;

        var routeValues = context.ActionDescriptor.RouteValues;
        if (!routeValues.TryGetValue("area", out var area) || area != "Operator")
            return;

        // Skip if action threw or returned 4xx/5xx
        if (result.Exception != null && !result.ExceptionHandled)
            return;

        var user = context.HttpContext.User;
        var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return;

        var userOperator = await _userOperatorRepo.GetByUserIdAsync(userId);
        if (userOperator == null)
            return;

        routeValues.TryGetValue("controller", out var controller);
        routeValues.TryGetValue("action", out var action);

        var log = new OperatorActionLog
        {
            OperatorId = userOperator.OperatorId,
            UserId     = userId,
            UserEmail  = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity?.Name ?? string.Empty,
            UserName   = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            Action     = GetReadableAction(controller ?? "", action ?? ""),
            OccurredAt = DateTime.UtcNow
        };

        _context.OperatorActionLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    private static string GetReadableAction(string controller, string action) =>
        (controller, action) switch
        {
            ("Satellite",    "Create")          => "Added satellite",
            ("Satellite",    "Edit")            => "Edited satellite",
            ("Satellite",    "Delete")          => "Deleted satellite",
            ("Satellite",    "RegenerateRisk")  => "Recalculated satellite risk",
            ("Alerts",       "Acknowledge")     => "Acknowledged alert",
            ("Alerts",       "EditMessage")     => "Edited alert message",
            ("Conjunctions", "Acknowledge")     => "Updated conjunction acknowledgement",
            ("Settings",     "UpdateOperator")  => "Updated operator profile",
            ("Settings",     "AddHelper")       => "Added team member",
            ("Settings",     "RemoveHelper")    => "Removed team member",
            ("Settings",     "SaveNotifications") => "Updated notification settings",
            _ => $"{controller} / {action}"
        };
}
