using System.Security.Claims;
using App.Application.Features.Satellites.DTOs;
using App.Application.Features.Satellites.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class SandboxController : Controller
{
    private readonly SaveDraftSatelliteUseCase _saveDraft;
    private readonly GetUserDraftsUseCase _getDrafts;
    private readonly DeleteDraftSatelliteUseCase _deleteDraft;

    public SandboxController(
        SaveDraftSatelliteUseCase saveDraft,
        GetUserDraftsUseCase getDrafts,
        DeleteDraftSatelliteUseCase deleteDraft)
    {
        _saveDraft = saveDraft;
        _getDrafts = getDrafts;
        _deleteDraft = deleteDraft;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? draftId = null)
    {
        var drafts = new List<DraftSatelliteDto>();

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = GetUserId();
            if (userId != Guid.Empty)
                drafts = (await _getDrafts.ExecuteAsync(userId)).ToList();
        }

        ViewBag.Drafts = drafts;
        ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated == true;

        if (draftId.HasValue)
            ViewBag.InitDraft = drafts.FirstOrDefault(d => d.Id == draftId.Value);

        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromForm] SaveDraftSatelliteRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m));
            TempData["ErrorMessage"] = string.Join(" | ", errors);
            return RedirectToAction(nameof(Index));
        }

        var userId = GetUserId();
        if (userId == Guid.Empty) return Forbid();

        try
        {
            var dto = await _saveDraft.ExecuteAsync(userId, request);
            TempData["SuccessMessage"] = $"Draft \"{dto.Name}\" saved.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Forbid();

        var deleted = await _deleteDraft.ExecuteAsync(userId, id);
        if (!deleted)
            TempData["ErrorMessage"] = "Draft not found.";
        else
            TempData["SuccessMessage"] = "Draft deleted.";

        return RedirectToAction(nameof(Index));
    }

    private Guid GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}
