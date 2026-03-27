using LMSModel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SQTWeb.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly IDbContextFactory<LMSContext> _dbFactory;

    public CustomersController(IDbContextFactory<LMSContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IActionResult> CustomersList(string? keyword, string? companyCode)
    {
        ViewBag.Keyword = keyword;
        ViewBag.CompanyCode = companyCode;

        await using var db = await _dbFactory.CreateDbContextAsync();
        var q = db.COM_View_CustomerLists.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(companyCode))
        {
            q = q.Where(x => x.CompanyCode == companyCode.Trim());
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            q = q.Where(x =>
                (x.CustomerCode ?? "").Contains(k) ||
                (x.CustomerName ?? "").Contains(k) ||
                (x.IDNo ?? "").Contains(k));
        }

        q = q.Where(x => x.StopUse != true);

        var items = await q.OrderBy(x => x.CustomerCode).Take(200).ToListAsync();
        return View(items);
    }

    // TODO: Customers come from a database view (Keyless). Full CRUD requires a base table.
    //       Uncomment and adapt when the writable entity/table is available.

    //[HttpGet]
    //public IActionResult CreateCustomer() => View(new COM_View_CustomerList());

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> CreateCustomer(COM_View_CustomerList model)
    //{
    //    if (!ModelState.IsValid) return View(model);
    //    // await using var db = await _dbFactory.CreateDbContextAsync();
    //    // db.Customers.Add(model);
    //    // await db.SaveChangesAsync();
    //    TempData["Success"] = "Customer created.";
    //    return RedirectToAction(nameof(CustomersList));
    //}

    //[HttpGet]
    //public async Task<IActionResult> EditCustomer(string id)
    //{
    //    if (string.IsNullOrWhiteSpace(id)) return NotFound();
    //    // await using var db = await _dbFactory.CreateDbContextAsync();
    //    // var entity = await db.Customers.FindAsync(id);
    //    // if (entity is null) return NotFound();
    //    // return View(entity);
    //    return NotFound();
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> EditCustomer(COM_View_CustomerList model)
    //{
    //    if (!ModelState.IsValid) return View(model);
    //    // await using var db = await _dbFactory.CreateDbContextAsync();
    //    // db.Customers.Update(model);
    //    // await db.SaveChangesAsync();
    //    TempData["Success"] = "Customer updated.";
    //    return RedirectToAction(nameof(CustomersList));
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> DeleteCustomer(string id)
    //{
    //    if (string.IsNullOrWhiteSpace(id)) return NotFound();
    //    // await using var db = await _dbFactory.CreateDbContextAsync();
    //    // var entity = await db.Customers.FindAsync(id);
    //    // if (entity is not null) { db.Customers.Remove(entity); await db.SaveChangesAsync(); }
    //    TempData["Success"] = "Customer deleted.";
    //    return RedirectToAction(nameof(CustomersList));
    //}
}
