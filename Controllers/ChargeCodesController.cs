using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQTWeb.Models.QTMS;

namespace SQTWeb.Controllers;

[Authorize]
public class ChargeCodesController : Controller
{
    public ChargeCodesController()
    {
    }

    public async Task<IActionResult> Index(string? keyword, bool? active)
    {
        ViewBag.Keyword = keyword;
        ViewBag.Active = active;

        var items = new List<COM_Ms_SalesQuotationCharge>();
        return View(items);
    }

    [HttpGet]
    public IActionResult Create() => View(new COM_Ms_SalesQuotationCharge());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(COM_Ms_SalesQuotationCharge model)
    {
        if (!ModelState.IsValid)
            return View(model);

        TempData["Success"] = "Charge code created (not persisted).";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        // TODO: load from DB
        return View(new COM_Ms_SalesQuotationCharge());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, COM_Ms_SalesQuotationCharge model)
    {
        if (!ModelState.IsValid)
            return View(model);

        TempData["Success"] = "Charge code updated (not persisted).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        TempData["Success"] = "Charge code deleted (not persisted).";
        return RedirectToAction(nameof(Index));
    }

    // JSON endpoint — used by the quotation form charge-code autocomplete
    [HttpGet]
    public async Task<IActionResult> All()
    {
        var items = new List<COM_Ms_SalesQuotationCharge>();
        return Json(items);
    }
}
