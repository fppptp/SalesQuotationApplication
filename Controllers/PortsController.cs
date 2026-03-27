using LMSModel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQTWeb.Services.Masters;

namespace SQTWeb.Controllers;

[Authorize]
public class PortsController : Controller
{
    private readonly IPortService _portService;
    private readonly IDbContextFactory<LMSContext> _dbFactory;

    public PortsController(IPortService portService, IDbContextFactory<LMSContext> dbFactory)
    {
        _portService = portService;
        _dbFactory = dbFactory;
    }

    public async Task<IActionResult> PortsList(string? keyword)
    {
        ViewBag.Keyword = keyword;

        var all = await _portService.GetOptionsAsync();

        var items = string.IsNullOrWhiteSpace(keyword)
            ? all
            : all.Where(x =>
                (x.Code ?? "").Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase) ||
                (x.Name ?? "").Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase) ||
                (x.CountryName ?? "").Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase))
              .ToList();

        return View(items);
    }

    // TODO: Ports come from a database view (Keyless). Full CRUD requires a base table.
    //       Uncomment and adapt when the writable entity/table is available.

    //[HttpGet]
    //public IActionResult CreatePort() => View(new COM_View_Port());

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> CreatePort(COM_View_Port model)
    //{
    //    if (!ModelState.IsValid) return View(model);
    //    // await using var db = await _dbFactory.CreateDbContextAsync();
    //    // db.Ports.Add(model);
    //    // await db.SaveChangesAsync();
    //    TempData["Success"] = "Port created.";
    //    return RedirectToAction(nameof(PortsList));
    //}

    //[HttpGet]
    //public async Task<IActionResult> EditPort(string id)
    //{
    //    if (string.IsNullOrWhiteSpace(id)) return NotFound();
    //    // await using var db = await _dbFactory.CreateDbContextAsync();
    //    // var entity = await db.Ports.FindAsync(id);
    //    // if (entity is null) return NotFound();
    //    // return View(entity);
    //    return NotFound();
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> EditPort(COM_View_Port model)
    //{
    //    if (!ModelState.IsValid) return View(model);
    //    // await using var db = await _dbFactory.CreateDbContextAsync();
    //    // db.Ports.Update(model);
    //    // await db.SaveChangesAsync();
    //    TempData["Success"] = "Port updated.";
    //    return RedirectToAction(nameof(PortsList));
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> DeletePort(string id)
    //{
    //    if (string.IsNullOrWhiteSpace(id)) return NotFound();
    //    // await using var db = await _dbFactory.CreateDbContextAsync();
    //    // var entity = await db.Ports.FindAsync(id);
    //    // if (entity is not null) { db.Ports.Remove(entity); await db.SaveChangesAsync(); }
    //    TempData["Success"] = "Port deleted.";
    //    return RedirectToAction(nameof(PortsList));
    //}
}
