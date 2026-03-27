using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QTMSModel.Models;

namespace SQTWeb.Controllers;

[Authorize]
public class UnitsController : Controller
{
    private readonly IDbContextFactory<QTMSContext> _dbFactory;

    public UnitsController(IDbContextFactory<QTMSContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IActionResult> UnitsList(string? keyword)
    {
        ViewBag.Keyword = keyword;

        await using var db = await _dbFactory.CreateDbContextAsync();
        var q = db.COM_Ms_UnitOfMeasures.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            q = q.Where(x =>
                (x.UnitCode ?? "").Contains(k) ||
                (x.UnitName ?? "").Contains(k));
        }

        var items = await q.OrderBy(x => x.UnitCode).ToListAsync();
        return View(items);
    }

    [HttpGet]
    public IActionResult CreateUnit() => View(new COM_Ms_UnitOfMeasure());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUnit(COM_Ms_UnitOfMeasure model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await using var db = await _dbFactory.CreateDbContextAsync();
        db.COM_Ms_UnitOfMeasures.Add(model);
        await db.SaveChangesAsync();

        TempData["Success"] = "Unit created.";
        return RedirectToAction(nameof(UnitsList));
    }

    [HttpGet]
    public async Task<IActionResult> EditUnit(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = await db.COM_Ms_UnitOfMeasures.FindAsync(id);
        if (entity is null) return NotFound();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUnit(COM_Ms_UnitOfMeasure model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await using var db = await _dbFactory.CreateDbContextAsync();
        db.COM_Ms_UnitOfMeasures.Update(model);
        await db.SaveChangesAsync();

        TempData["Success"] = "Unit updated.";
        return RedirectToAction(nameof(UnitsList));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUnit(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = await db.COM_Ms_UnitOfMeasures.FindAsync(id);
        if (entity is not null)
        {
            db.COM_Ms_UnitOfMeasures.Remove(entity);
            await db.SaveChangesAsync();
        }

        TempData["Success"] = "Unit deleted.";
        return RedirectToAction(nameof(UnitsList));
    }
}
