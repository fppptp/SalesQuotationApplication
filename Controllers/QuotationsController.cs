using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SQTWeb.Models;
using SQTWeb.Models.LMS;
using SQTWeb.Models.QTMS;

namespace SQTWeb.Controllers;

[Authorize]
public class QuotationsController : Controller
{
    private readonly QTMSContext _context;
    private readonly SQTWeb.Models.FMS.FMSContext? _fmsContext;

    public QuotationsController(QTMSContext context, SQTWeb.Models.FMS.FMSContext? fmsContext = null)
    {
        _context = context;
        _fmsContext = fmsContext;
    }

    public async Task<IActionResult> Index(string? keyword, string? status)
    {
        ViewBag.Keyword = keyword;
        ViewBag.Status = status;

        // Query quotations from QTMSContext and project to the existing view model
        var q = _context.COM_Trs_SalesQuotations
            .Include(x => x.COM_Trs_SalesQuotationShipmentDetails)
            .Include(x => x.COM_Trs_SalesQuotationFreightCharges)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            q = q.Where(x => x.QuotationNo.Contains(k) || (x.CustomerName ?? "").Contains(k) || (x.PortName ?? "").Contains(k));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            q = q.Where(x => x.StatusName == status);
        }

        var items = await q.OrderByDescending(x => x.QuotationDate).ToListAsync();
            //.Select(x => new QuoteListItemViewModel
            //{
            //    // Map available fields. Adjust names if your view model differs.
            //    QuoteNo = x.QuotationNo,
            //    QuoteDate = x.QuotationDate ?? DateTime.Today,
            //    CustomerName = x.CustomerName,
            //    Mode = "",  
            //    Origin = "",
            //    Destination = x.PortName,
            //    Status = x.StatusName,
            //    CurrencyCode = x.CurrencyCode,
            //    GrandTotal = x.GrandTotalFrieghtAmountTHB ?? 0,
            //    ProfitTotal = 0,
            //    RevisionNo = 1
            //})
            //.ToListAsync();

        return View(items);
        // Load currencies from cache populated at startup
        var currencies = SQTWeb.Models.FMS.CurrencyCache.Currencies
            .Select(c => new SelectListItem { Value = c.CurrencyCode, Text = c.CurrencyCode + (string.IsNullOrWhiteSpace(c.CurrencyName) ? string.Empty : " - " + c.CurrencyName) })
            .ToList();

        if (!currencies.Any())
        {
            // fallback: if cache is empty and we have an FMS context, refresh it
            if (_fmsContext != null)
            {
                await SQTWeb.Models.FMS.CurrencyCache.RefreshAsync(_fmsContext);
                currencies = SQTWeb.Models.FMS.CurrencyCache.Currencies
                    .Select(c => new SelectListItem { Value = c.CurrencyCode, Text = c.CurrencyCode + (string.IsNullOrWhiteSpace(c.CurrencyName) ? string.Empty : " - " + c.CurrencyName) })
                    .ToList();
            }
        }

        ViewBag.Currencies = new SelectList(currencies, "Value", "Text");
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDropdownsAsync();
        var model = new COM_Trs_SalesQuotation
        {
            QuotationDate = DateTime.Today,
            COM_Trs_SalesQuotationFreightCharges = new List<COM_Trs_SalesQuotationFreightCharge>
            {
                new COM_Trs_SalesQuotationFreightCharge { LineNo = 1, UnitPrice = 0 }
            }
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(COM_Trs_SalesQuotation model)
    {
        if (model.COM_Trs_SalesQuotationFreightCharges == null || !model.COM_Trs_SalesQuotationFreightCharges.Any())
            ModelState.AddModelError(string.Empty, "Please add at least one charge line.");

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(model);
        }

        model.QuotationNo ??= $"QTN{DateTime.Today:yyyyMM}-{Guid.NewGuid():N}";
        _context.COM_Trs_SalesQuotations.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Quotation created successfully.";
        return RedirectToAction(nameof(Details), new { id = model.QuotationNo });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var entity = await _context.COM_Trs_SalesQuotations
            .Include(x => x.COM_Trs_SalesQuotationFreightCharges)
            .Include(x => x.COM_Trs_SalesQuotationShipmentDetails)
            .FirstOrDefaultAsync(x => x.QuotationNo == id);

        if (entity is null) return NotFound();

        await LoadDropdownsAsync();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(COM_Trs_SalesQuotation model)
    {
        // Basic validation using the EF entity model
        if (model == null) return BadRequest();

        if (model.COM_Trs_SalesQuotationFreightCharges == null || !model.COM_Trs_SalesQuotationFreightCharges.Any())
        {
            ModelState.AddModelError(string.Empty, "Please add at least one charge line.");
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(model);
        }

        // find persisted entity
        if (string.IsNullOrWhiteSpace(model.QuotationNo)) return BadRequest();

        var entity = await _context.COM_Trs_SalesQuotations
            .Include(x => x.COM_Trs_SalesQuotationFreightCharges)
            .FirstOrDefaultAsync(x => x.QuotationNo == model.QuotationNo);

        if (entity is null) return NotFound();

        // update simple fields
        entity.QuotationDate = model.QuotationDate;
        entity.CustomerName = model.CustomerName;
        entity.CurrencyCode = model.CurrencyCode;
        entity.ExchangeRate = model.ExchangeRate;
        entity.StatusName = model.StatusName;
        entity.GrandTotalFrieghtAmountTHB = model.GrandTotalFrieghtAmountTHB;

        // replace freight charges: remove existing and add new ones from posted model
        _context.COM_Trs_SalesQuotationFreightCharges.RemoveRange(entity.COM_Trs_SalesQuotationFreightCharges ?? Enumerable.Empty<COM_Trs_SalesQuotationFreightCharge>());

        var newCharges = (model.COM_Trs_SalesQuotationFreightCharges ?? Enumerable.Empty<COM_Trs_SalesQuotationFreightCharge>())
            .Select((c, idx) => new COM_Trs_SalesQuotationFreightCharge
            {
                CompanyCode = entity.CompanyCode,
                QuotationNo = entity.QuotationNo,
                LineNo = idx + 1,
                ChargeCode = c.ChargeCode,
                ChargeName = c.ChargeName,
                UnitPrice = c.UnitPrice,
                CurrencyCode = c.CurrencyCode
            }).ToList();

        entity.COM_Trs_SalesQuotationFreightCharges = newCharges;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Quotation updated successfully.";
        return RedirectToAction(nameof(Details), new { id = entity.QuotationNo });
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var entity = await _context.COM_Trs_SalesQuotations
            .Include(x => x.COM_Trs_SalesQuotationFreightCharges)
            .Include(x => x.COM_Trs_SalesQuotationShipmentDetails)
            .FirstOrDefaultAsync(x => x.QuotationNo == id);

        if (entity is null) return NotFound();

        //var model = new COM_Trs_SalesQuotations
        //{
        //    QuoteNo = entity.QuotationNo,
        //    QuoteDate = entity.QuotationDate,
        //    ValidUntil = entity.QuotationDate?.AddDays(30) ?? DateTime.Today.AddDays(30),
        //    CustomerName = entity.CustomerName,
        //    CompanyCode = entity.CompanyCode,
        //    CurrencyCode = entity.CurrencyCode,
        //    ExchangeRate = entity.ExchangeRate ?? 1,
        //    Status = entity.StatusName,
        //        GrandTotal = entity.GrandTotalFrieghtAmountTHB ?? 0,
        //    Items = entity.COM_Trs_SalesQuotationFreightCharges.Select(c => new QuoteItemInputModel
        //    {
        //        SortOrder = c.LineNo,
        //        ChargeCode = c.ChargeCode,
        //        ChargeName = c.ChargeName,
        //        UnitPrice = c.UnitPrice ?? 0,
        //        Quantity = 1
        //    }).ToList()
        //};

        return View(entity);
    }

    [HttpGet]
    public async Task<IActionResult> Print(string id)
    {
        return await Details(id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revise(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        // Basic revision: duplicate quotation with new QuotationNo and increment revision number if tracked
        var src = await _context.COM_Trs_SalesQuotations
            .Include(x => x.COM_Trs_SalesQuotationFreightCharges)
            .FirstOrDefaultAsync(x => x.QuotationNo == id);

        if (src is null) return NotFound();

        var newQuotationNo = $"{src.QuotationNo}-R{DateTime.UtcNow.Ticks}";
        var copy = new COM_Trs_SalesQuotation
        {
            CompanyCode = src.CompanyCode,
            QuotationNo = newQuotationNo,
            QuotationDate = DateTime.Today,
            CustomerName = src.CustomerName,
            CurrencyCode = src.CurrencyCode,
            ExchangeRate = src.ExchangeRate,
            StatusName = "Draft",
            GrandTotalFrieghtAmountTHB = src.GrandTotalFrieghtAmountTHB
        };

        foreach (var c in src.COM_Trs_SalesQuotationFreightCharges)
        {
            copy.COM_Trs_SalesQuotationFreightCharges.Add(new COM_Trs_SalesQuotationFreightCharge
            {
                CompanyCode = copy.CompanyCode,
                QuotationNo = copy.QuotationNo,
                LineNo = c.LineNo,
                ChargeCode = c.ChargeCode,
                ChargeName = c.ChargeName,
                UnitPrice = c.UnitPrice,
                CurrencyCode = c.CurrencyCode
            });
        }

        _context.COM_Trs_SalesQuotations.Add(copy);
        await _context.SaveChangesAsync();

        TempData["Success"] = "New revision created. You are now editing the revised quotation.";
        return RedirectToAction(nameof(Edit), new { id = copy.QuotationNo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Copy(string id)
    {
        return await Revise(id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(string id, string status)
    {
        if (string.IsNullOrWhiteSpace(status) || !AppLists.Statuses.Contains(status))
        {
            TempData["Error"] = "Invalid status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var entity = await _context.COM_Trs_SalesQuotations.FirstOrDefaultAsync(x => x.QuotationNo == id);
        if (entity is null) return NotFound();
        entity.StatusName = status;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Quotation status changed to {status}.";
        return RedirectToAction(nameof(Details), new { id = entity.QuotationNo });
    }

    [HttpGet]
    public async Task<IActionResult> CustomersByCompany(string? company)
    {
        if (string.IsNullOrWhiteSpace(company))
        { return Json(Array.Empty<COM_View_CustomerList>()); }

        // If View_Customer is not available via EF model, attempt to read distinct customers from quotations
        var customers = await _context.COM_Trs_SalesQuotations
            .Where(x => x.CompanyCode == company.Trim())           
            .Distinct()
            .ToListAsync();

        return Json(customers);
    }

    private async Task LoadDropdownsAsync()
    {
        //var companies = await _context.COM_Trs_SalesQuotations
        //    .Where(x => !string.IsNullOrEmpty(x.CompanyCode))
        //    .Select(x => x.CompanyCode.Trim())
        //    .Distinct()
        //    .OrderBy(c => c)
        //    .ToListAsync();

        //var list = companies.Select(c => new CompanyLookupViewModel { CompanyCode = c, DisplayName = c }).ToList();
        //ViewBag.Companies = new SelectList(list, nameof(CompanyLookupViewModel.CompanyCode), nameof(CompanyLookupViewModel.DisplayName));
    }

    private static void NormalizeBeforeValidate(COM_Trs_SalesQuotation model)
    {
        //model.Mode = string.IsNullOrWhiteSpace(model.Mode) ? "Sea" : model.Mode.Trim();
        //model.ServiceType = string.IsNullOrWhiteSpace(model.ServiceType) ? "Door to Door" : model.ServiceType.Trim();
        //model.Incoterm = string.IsNullOrWhiteSpace(model.Incoterm) ? "EXW" : model.Incoterm.Trim();
        //model.CurrencyCode = string.IsNullOrWhiteSpace(model.CurrencyCode) ? "THB" : model.CurrencyCode.Trim();
        //model.Status = string.IsNullOrWhiteSpace(model.Status) ? "Draft" : model.Status.Trim();
        //model.Items = (model.Items ?? new List<QuoteItemInputModel>())
        //    .Where(x => !string.IsNullOrWhiteSpace(x.ChargeCode) || !string.IsNullOrWhiteSpace(x.ChargeName))
        //    .Select((x, index) =>
        //    {
        //        x.SortOrder = index + 1;
        //        x.ChargeCode = (x.ChargeCode ?? string.Empty).Trim();
        //        x.ChargeName = (x.ChargeName ?? string.Empty).Trim();
        //        x.ChargeBasis = string.IsNullOrWhiteSpace(x.ChargeBasis) ? "Lump Sum" : x.ChargeBasis.Trim();
        //        x.ChargeCategory = string.IsNullOrWhiteSpace(x.ChargeCategory) ? "Freight" : x.ChargeCategory.Trim();
        //        return x;
        //    })
        //    .ToList();
    }

    private void ClearStaleItemModelState(int keepCount)
    {
        foreach (var key in ModelState.Keys.ToList().Where(k => k.StartsWith("Items[")))
        {
            var bracket = key.IndexOf(']');
            if (bracket > 7 &&
                int.TryParse(key[6..bracket], out var idx) &&
                idx >= keepCount)
            {
                ModelState.Remove(key);
            }
        }
    }
}