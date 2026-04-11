using App.Domain.Entities;
using App.Domain.Entities.Enums;
using App.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Areas.Admin.ViewModels;

namespace WebApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var satellites   = await _context.Satellites.CountAsync();
        var activeSats   = await _context.Satellites.CountAsync(s => s.Status == ESatelliteStatus.Active);
        var operators    = await _context.Operators.CountAsync();
        var conjunctions = await _context.ConjunctionEvents.CountAsync();
        var highRisk     = await _context.ConjunctionEvents.CountAsync(c =>
            c.CollisionProbability > 0.01 || c.MissDistance < 1000);
        var pending      = await _context.ConjunctionEvents.CountAsync(c =>
            c.Status == EConjunctionEventStatus.Pending);
        var totalAlerts  = await _context.Alerts.CountAsync();
        var unackAlerts  = await _context.Alerts.CountAsync(a => !a.IsAcknowledged);
        var totalUsers   = await _context.Users.CountAsync();
        var tleRecords   = await _context.TleRecords.CountAsync();

        var recentEventEntities = await _context.ConjunctionEvents
            .Include(c => c.PrimaryObject)
            .Include(c => c.SecondaryObject)
            .OrderByDescending(c => c.CreatedAt)
            .Take(8)
            .ToListAsync();

        var recentEvents = recentEventEntities.Select(c => new AdminDashboardViewModel.RecentEventRow
        {
            Id              = c.Id,
            PrimaryObject   = c.PrimaryObject is Satellite ps ? ps.Name.ToString() : $"DEBRIS-{c.PrimaryObject?.NoradId}",
            SecondaryObject = c.SecondaryObject is Satellite ss ? ss.Name.ToString() : $"DEBRIS-{c.SecondaryObject?.NoradId}",
            Tca             = c.TimeOfClosestApproach,
            MissDistance    = c.MissDistance,
            Status          = c.Status.ToString(),
            IsHighRisk      = c.CollisionProbability > 0.01 || c.MissDistance < 1000
        }).ToList();

        var recentAlerts = await _context.Alerts
            .OrderByDescending(a => a.CreatedAt)
            .Take(8)
            .Select(a => new AdminDashboardViewModel.RecentAlertRow
            {
                Id             = a.Id,
                Severity       = a.Severity.ToString(),
                Message        = a.Message != null ? a.Message.ToString() : string.Empty,
                IsAcknowledged = a.IsAcknowledged,
                CreatedAt      = a.CreatedAt
            })
            .ToListAsync();

        var vm = new AdminDashboardViewModel
        {
            TotalSatellites      = satellites,
            ActiveSatellites     = activeSats,
            TotalOperators       = operators,
            TotalConjunctions    = conjunctions,
            HighRiskEvents       = highRisk,
            PendingEvents        = pending,
            TotalAlerts          = totalAlerts,
            UnacknowledgedAlerts = unackAlerts,
            TotalUsers           = totalUsers,
            TotalTleRecords      = tleRecords,
            RecentEvents         = recentEvents,
            RecentAlerts         = recentAlerts
        };

        return View(vm);
    }
}
