using FMSModel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQTWeb.Services.Agents;
using SQTWeb.Services.Ports;
using SQTWeb.Services.QuotationStatus;
using SQTWeb.Services.UnitOfMeasure;

namespace SQTWeb.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly ICurrencyService _currencyService;
    private readonly IChargeService _chargeService;
    private readonly IQuotationStatusService _statusService;
    private readonly IUnitOfMeasureService _uomService;
    private readonly IAgentService _agentService;
    private readonly IPortService _portService;

    public AdminController(
        ICompanyService companyService,
        ICurrencyService currencyService,
        IChargeService chargeService,
        IQuotationStatusService statusService,
        IUnitOfMeasureService uomService,
        IAgentService agentService,
        IPortService portService)
    {
        _companyService = companyService;
        _currencyService = currencyService;
        _chargeService = chargeService;
        _statusService = statusService;
        _uomService = uomService;
        _agentService = agentService;
        _portService = portService;
    }

    [HttpPost]
    [Route("Admin/RefreshAllCachesJson")]
    public async Task<IActionResult> RefreshAllCachesJson()
    {
        try
        {
            await _companyService.GetOptionsAsync(forceRefresh: true);
            await _currencyService.GetOptionsAsync(forceRefresh: true);
            await _chargeService.GetOptionsAsync(forceRefresh: true);
            await _statusService.GetOptionsAsync(forceRefresh: true);
            await _uomService.GetOptionsAsync(forceRefresh: true);
            await _agentService.GetOptionsAsync(forceRefresh: true);
            await _portService.GetOptionsAsync(forceRefresh: true);
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
    public async Task<IActionResult> CompanyOptionsFragment(string? name = "CompanyCode", string? value = null, string cssClass = "form-control")
    {
        var options = (IReadOnlyList<COM_MS_Company>)await _companyService.GetOptionsAsync();
        ViewData["Name"] = name;
        ViewData["Value"] = value;
        ViewData["CssClass"] = cssClass;
        return PartialView("~/Views/Shared/Components/CompanySearchLookup/Default.cshtml", options);
    }
}
