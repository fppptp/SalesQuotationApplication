using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QTMSModel.Models;
using SQTWeb.Services;

namespace SQTWeb.Controllers;

[Authorize]
public class ChargeCodesController : Controller
{
    private readonly IDbContextFactory<QTMSContext> _dbFactory;
    private readonly IChargeService _chargeService;

    public ChargeCodesController(IDbContextFactory<QTMSContext> dbFactory, IChargeService chargeService)
    {
        _dbFactory = dbFactory;
        _chargeService = chargeService;
    }

    public async Task<IActionResult> ChargesList(string? keyword)
    {
        ViewBag.Keyword = keyword;

        await using var db = await _dbFactory.CreateDbContextAsync();
        var q = db.COM_Ms_SalesQuotationCharges.AsNoTracking()
            .Where(x => x.Void != true);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            q = q.Where(x =>
                (x.ChargeCode ?? "").Contains(k) ||
                (x.ChargeName ?? "").Contains(k));
        }

        var items = await q
            .OrderBy(x => x.CompanyCode).ThenBy(x => x.Department).ThenBy(x => x.ChargeCode)
            .ToListAsync();
        return View(items);
    }

    [HttpGet]
    public IActionResult CreateCharge() => View(new COM_Ms_SalesQuotationCharge());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCharge(COM_Ms_SalesQuotationCharge model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await using var db = await _dbFactory.CreateDbContextAsync();
        db.COM_Ms_SalesQuotationCharges.Add(model);
        await db.SaveChangesAsync();
        await _chargeService.ClearCacheAsync();

        TempData["Success"] = "Charge code created.";
        return RedirectToAction(nameof(ChargesList));
    }

    [HttpGet]
    public async Task<IActionResult> EditCharge(string companyCode, string department, string quotationType, string chargeCode)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = await db.COM_Ms_SalesQuotationCharges.FindAsync(companyCode, department, quotationType, chargeCode);
        if (entity is null) return NotFound();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCharge(COM_Ms_SalesQuotationCharge model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await using var db = await _dbFactory.CreateDbContextAsync();
        db.COM_Ms_SalesQuotationCharges.Update(model);
        await db.SaveChangesAsync();
        await _chargeService.ClearCacheAsync();

        TempData["Success"] = "Charge code updated.";
        return RedirectToAction(nameof(ChargesList));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCharge(string companyCode, string department, string quotationType, string chargeCode)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = await db.COM_Ms_SalesQuotationCharges.FindAsync(companyCode, department, quotationType, chargeCode);
        if (entity is not null)
        {
            db.COM_Ms_SalesQuotationCharges.Remove(entity);
            await db.SaveChangesAsync();
            await _chargeService.ClearCacheAsync();
        }

        TempData["Success"] = "Charge code deleted.";
        return RedirectToAction(nameof(ChargesList));
    }
}
