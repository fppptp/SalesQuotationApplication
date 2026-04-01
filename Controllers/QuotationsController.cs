using SQTWeb.Navigation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QTMSModel.Models;
using SQTWeb.Services.Quotations;

namespace SQTWeb.Controllers;

[Authorize]
public class QuotationsController : Controller
{
    private readonly QTMSContext _qtmsContext;
    private readonly IQuotationAppService _quotationAppService;
    private readonly IQuotationCalculationOrchestrator _calculationOrchestrator;

    public QuotationsController(
        QTMSContext qtmsContext,
        IQuotationAppService quotationAppService,
        IQuotationCalculationOrchestrator calculationOrchestrator)
    {
        _qtmsContext = qtmsContext;
        _quotationAppService = quotationAppService;
        _calculationOrchestrator = calculationOrchestrator;
    }

    [HttpGet]
    public async Task<IActionResult> QuotationsList(string? keyword, string? status)
    {
        ViewBag.Keyword = keyword;
        ViewBag.Status = status;
        var q = _qtmsContext.COM_Trs_SalesQuotations.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            q = q.Where(x => EF.Functions.Like(x.QuotationNo, $"%{k}%"));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            q = q.Where(x => x.StatusName == status);
        }

        var items = q.OrderByDescending(x => x.QuotationDate).ToList();

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> CreateQuotation()
    {
        var model = new COM_Trs_SalesQuotation
        {
            QuotationDate = DateTime.Today,
            ExchangeRate = 1m,
            IsVoid = false,
            GrandTotalGrossWeightKg = 0m,
            GrandTotalVolumeCBM = 0m,
            GrandTotalFrieghtAmountDest = 0m,
            GrandTotalFrieghtAmountTHB = 0m,
            COM_Trs_SalesQuotationFreightCharges = new List<COM_Trs_SalesQuotationFreightCharge>
            {
                new COM_Trs_SalesQuotationFreightCharge
                {
                    No = 1,
                    UnitPrice = 0m,
                    MarginAmount = 0m,
                    ExchangeRateToHeader = 1m
                }
            }
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuotation(COM_Trs_SalesQuotation model)
    {
        try
        {
            await _quotationAppService.SaveAsync(model, FormMode.Add);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Unable to save quotation. " + ex.Message;
            return View(model);
        }

        TempData["Success"] = "Quotation created successfully.";
        return RedirectToAction(nameof(QuotationDetails), new { id = model.QuotationNo });
    }

    [HttpGet]
    public async Task<IActionResult> EditQuotation(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var entity = await _qtmsContext.COM_Trs_SalesQuotations
            .Include(x => x.COM_Trs_SalesQuotationFreightCharges)
                .ThenInclude(c => c.COM_Trs_SalesQuotationFreightChargePriceTiers)
            .Include(x => x.COM_Trs_SalesQuotationShipmentDetails)
            .FirstOrDefaultAsync(x => x.QuotationNo == id);

        if (entity is null) return NotFound();

        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuotation(COM_Trs_SalesQuotation model)
    {
        if (model == null) return BadRequest();

        try
        {
            await _quotationAppService.SaveAsync(model, FormMode.Edit);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Unable to save quotation. " + ex.Message;
            return View(model);
        }

        TempData["Success"] = "Quotation created successfully.";
        return RedirectToAction(nameof(QuotationDetails), new { id = model.QuotationNo });
    }

    [HttpGet]
    public async Task<IActionResult> QuotationDetails(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var entity = await _qtmsContext.COM_Trs_SalesQuotations
            .Include(x => x.COM_Trs_SalesQuotationFreightCharges)
                .ThenInclude(c => c.COM_Trs_SalesQuotationFreightChargePriceTiers)
            .Include(x => x.COM_Trs_SalesQuotationShipmentDetails)
            .FirstOrDefaultAsync(x => x.QuotationNo == id);

        if (entity is null) return NotFound();

        return View(entity);
    }

    [HttpGet]
    public async Task<IActionResult> PrintQuotation(string id)
    {
        return await QuotationDetails(id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CalculateHeaderDimension([FromBody] COM_Trs_SalesQuotation entity)
    {
        try
        {
            if (entity == null)
            {
                return BadRequest("Entity is required.");
            }

            _calculationOrchestrator.RecalculateAll(entity);

            return Ok(entity);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RecalculateShipmentDetail([FromBody] COM_Trs_SalesQuotation entity, [FromQuery] int detailIndex)
    {
        try
        {
            if (entity == null)
            {
                return BadRequest("Entity is required.");
            }

            var details = entity.COM_Trs_SalesQuotationShipmentDetails;
            if (details == null || detailIndex < 0 || detailIndex >= details.Count)
            {
                return BadRequest("Shipment detail index is out of range.");
            }

            var changedDetail = details.ElementAt(detailIndex);
            _calculationOrchestrator.RecalculateAfterShipmentDetailChanged(entity, changedDetail);

            return Ok(entity);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CopyQuotation(string id)
    {
        return null;// await ReviseQuotation(id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeQuotationStatus(string id, string status)
    {
        try
        {
            await _quotationAppService.ChangeStatusAsync(id, status);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(QuotationDetails), new { id });
        }

        TempData["Success"] = $"Quotation status changed to {status}.";
        return RedirectToAction(nameof(QuotationDetails), new { id });
    }
}