using FMSModel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services.Masters;

namespace SQTWeb.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly ICurrencyService _currencyService;

    public AdminController(ICompanyService companyService, ICurrencyService currencyService)
    {
        _companyService = companyService;
        _currencyService = currencyService;
    }

    [HttpPost]
    [Route("Admin/RefreshAllCachesJson")]
    public async Task<IActionResult> RefreshAllCachesJson()
    {
        try
        {
            await _companyService.GetOptionsAsync(forceRefresh: true);
            await _currencyService.GetOptionsAsync(forceRefresh: true);
            return Json(new { ok = true });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, error = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshCompanyCache()
    {
        await _companyService.GetOptionsAsync(forceRefresh: true);
        return RedirectToAction("CreateQuotation", "Quotations");
    }

    [HttpPost]
    [Route("Admin/RefreshCompanyCacheJson")]
    public async Task<IActionResult> RefreshCompanyCacheJson()
    {
        await _companyService.GetOptionsAsync(forceRefresh: true);
        return Json(new { ok = true });
    }

    [HttpGet]
    [Route("Admin/CompanyOptionsFragment")]
    public async Task<IActionResult> CompanyOptionsFragment(string? name = "CompanyCode", string? value = null, string cssClass = "form-select")
    {
        var options = (IReadOnlyList<COM_MS_Company>)await _companyService.GetOptionsAsync();
        ViewData["Name"] = name;
        ViewData["Value"] = value;
        ViewData["CssClass"] = cssClass;
        return PartialView("~/Views/Shared/Components/CompanySelect/Default.cshtml", options);
    }
}
