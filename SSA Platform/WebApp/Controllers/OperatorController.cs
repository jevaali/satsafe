using System.Globalization;
using App.Application.DTO;
using App.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Controllers;

public class OperatorController : Controller
{
    private readonly AddOperatorUseCase _useCase;

    public OperatorController(AddOperatorUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Countries = GetCountrySelectList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddOperatorRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Countries = GetCountrySelectList(request.Country);
            return View(request);
        }

        var id = await _useCase.Execute(request);

        return RedirectToAction("Index", "Home");
    }

    private static List<SelectListItem> GetCountrySelectList(string? selectedCode = null)
    {
        var seen = new HashSet<string>();
        var items = CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .Select(c =>
            {
                try { return new RegionInfo(c.Name); }
                catch { return null; }
            })
            .Where(r => r != null && seen.Add(r.TwoLetterISORegionName))
            .OrderBy(r => r!.DisplayName)
            .Select(r => new SelectListItem
            {
                Value    = r!.TwoLetterISORegionName,
                Text     = r.DisplayName,
                Selected = string.Equals(r.TwoLetterISORegionName, selectedCode,
                                         StringComparison.OrdinalIgnoreCase)
            })
            .ToList();

        items.Insert(0, new SelectListItem { Value = "", Text = "— Select country —" });
        return items;
    }
}