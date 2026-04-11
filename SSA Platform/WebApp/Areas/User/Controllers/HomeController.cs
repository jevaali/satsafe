using System.Security.Claims;
using App.Application.DTO;
using App.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.User.Controllers;

[Area("User")]
[Authorize(Roles = "User")]
public class HomeController : Controller
{
    private readonly GetUserDraftsUseCase _getDrafts;
    private readonly SaveDraftSatelliteUseCase _saveDraft;
    private readonly DeleteDraftSatelliteUseCase _deleteDraft;

    public HomeController(
        GetUserDraftsUseCase getDrafts,
        SaveDraftSatelliteUseCase saveDraft,
        DeleteDraftSatelliteUseCase deleteDraft)
    {
        _getDrafts = getDrafts;
        _saveDraft = saveDraft;
        _deleteDraft = deleteDraft;
    }

    // GET /User/Home/Index
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var drafts = await GetDraftsAsync();
        return View(drafts);
    }

    // POST /User/Home/Save
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromForm] SaveDraftSatelliteRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(" | ",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
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

    // POST /User/Home/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Forbid();

        var deleted = await _deleteDraft.ExecuteAsync(userId, id);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Draft deleted."
            : "Draft not found.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<DraftSatelliteDto>> GetDraftsAsync()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return [];
        return (await _getDrafts.ExecuteAsync(userId)).ToList();
    }

    private Guid GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}
