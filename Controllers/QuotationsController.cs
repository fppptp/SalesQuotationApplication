using AppLib;
using AppLib.DocumentControl;
using AppLib.Mode;
using COMMONModel.Models;
using FMSModel.Models;
using LMSModel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QTMSModel.Models;
using SQTWeb.Cache;
using SQTWeb.DTOModels;
using SQTWeb.Models;
using System.Text;
using System.Transactions;
using UtilityLib.Extensions;

namespace SQTWeb.Controllers;

[Authorize]
public class QuotationsController : Controller
{
    private readonly QTMSContext qtmsContext;
    private readonly FMSContext fmsContext;
    private readonly COMMONContext commonContext;

    private string SeriesNo_TableName => qtmsContext.COM_Trs_SalesQuotations.GetEFCoreTableName();
    private string SeriesNo_ColumnName => qtmsContext.COM_Trs_SalesQuotations.GetEFCoreColumnName(x => x.QuotationNo);
    private string SeriesNo_FormatType => SeriesNo.Format.PrefixY2_M2_No.ToString();

    public QuotationsController(QTMSContext qtmsContextInput, FMSContext fmsContextInput, COMMONContext commonContextInput)
    {
        qtmsContext = qtmsContextInput;
        fmsContext = fmsContextInput;
        commonContext = commonContextInput;
    }

    //public async Task<IActionResult> List(string? keyword, string? status)
    //{
    //    ViewBag.Keyword = keyword;
    //    ViewBag.Status = status;

    //    // Query quotations from QTMSContext and project to the existing view model
    //    var q = qtmsContext.COM_Trs_SalesQuotations
    //        .Include(x => x.COM_Trs_SalesQuotationShipmentDetails)
    //        .Include(x => x.COM_Trs_SalesQuotationFreightCharges)
    //        .AsQueryable();

    //    if (!string.IsNullOrWhiteSpace(keyword))
    //    {
    //        var k = keyword.Trim();
    //        q = q.Where(x => x.QuotationNo.Contains(k) || (x.CustomerName ?? "").Contains(k) || (x.PortName ?? "").Contains(k));
    //    }

    //    if (!string.IsNullOrWhiteSpace(status))
    //    {
    //        q = q.Where(x => x.StatusName == status);
    //    }

    //    var items = await q.OrderByDescending(x => x.QuotationDate).ToListAsync();
    //    //.Select(x => new QuoteListItemViewModel
    //    //{
    //    //    // Map available fields. Adjust names if your view model differs.
    //    //    QuoteNo = x.QuotationNo,
    //    //    QuoteDate = x.QuotationDate ?? DateTime.Today,
    //    //    CustomerName = x.CustomerName,
    //    //    Mode = "",  
    //    //    Origin = "",
    //    //    Destination = x.PortName,
    //    //    Status = x.StatusName,
    //    //    CurrencyCode = x.CurrencyCode,
    //    //    GrandTotal = x.GrandTotalFrieghtAmountTHB ?? 0,
    //    //    ProfitTotal = 0,
    //    //    RevisionNo = 1
    //    //})
    //    //.ToListAsync();

    //    return View(items);
    //    //// Load currencies from cache populated at startup
    //    //var currencies = CurrencyCache.Currencies
    //    //    .Select(c => new SelectListItem { Value = c.CurrencyCode, Text = c.CurrencyCode + (string.IsNullOrWhiteSpace(c.CurrencyName) ? string.Empty : " - " + c.CurrencyName) })
    //    //    .ToList();

    //    //if (!currencies.Any())
    //    //{
    //    //    // fallback: if cache is empty and we have an FMS context, refresh it
    //    //    if (fmsContext != null)
    //    //    {
    //    //        await CurrencyCache.RefreshAsync(fmsContext);
    //    //        currencies = CurrencyCache.Currencies
    //    //            .Select(c => new SelectListItem { Value = c.CurrencyCode, Text = c.CurrencyCode + (string.IsNullOrWhiteSpace(c.CurrencyName) ? string.Empty : " - " + c.CurrencyName) })
    //    //            .ToList();
    //    //    }
    //    //}

    //    //ViewBag.Currencies = new SelectList(currencies, "Value", "Text");
    //}

    [HttpGet]
    public async Task<IActionResult> QuotationsList(string? keyword, string? status)
    {
        ViewBag.Keyword = keyword;
        ViewBag.Status = status;
        var q = qtmsContext.COM_Trs_SalesQuotations.AsEnumerable();

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
        await LoadDropdownsAsync();
        var model = new COM_Trs_SalesQuotation
        {
            QuotationDate = DateTime.Today,
            COM_Trs_SalesQuotationFreightCharges = new List<COM_Trs_SalesQuotationFreightCharge>
            {
                new COM_Trs_SalesQuotationFreightCharge { No = 1, UnitPrice = 0 }
            }
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuotation(COM_Trs_SalesQuotation model)
    {
        //model.QuotationNo ??= $"QTN{DateTime.Today:yyyyMM}-{Guid.NewGuid():N}";

        try
        {
            ValidateQuotation(ref model);
            SaveCore(ref model, FormMode.Add);
            //qtmsContext.COM_Trs_SalesQuotations.Add(model);
            //await qtmsContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Unable to save quotation. " + ex.Message;
            await LoadDropdownsAsync();
            return View(model);
        }

        TempData["Success"] = "Quotation created successfully.";
        return RedirectToAction(nameof(QuotationDetails), new { id = model.QuotationNo });
    }

    [HttpGet]
    public async Task<IActionResult> EditQuotation(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var entity = await qtmsContext.COM_Trs_SalesQuotations
            .Include(x => x.COM_Trs_SalesQuotationFreightCharges)
            .Include(x => x.COM_Trs_SalesQuotationShipmentDetails)
            .FirstOrDefaultAsync(x => x.QuotationNo == id);

        if (entity is null) return NotFound();

        await LoadDropdownsAsync();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuotation(COM_Trs_SalesQuotation model)
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

        var entity = await qtmsContext.COM_Trs_SalesQuotations
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
        qtmsContext.COM_Trs_SalesQuotationFreightCharges.RemoveRange(entity.COM_Trs_SalesQuotationFreightCharges ?? Enumerable.Empty<COM_Trs_SalesQuotationFreightCharge>());

        var newCharges = (model.COM_Trs_SalesQuotationFreightCharges ?? Enumerable.Empty<COM_Trs_SalesQuotationFreightCharge>())
            .Select((c, idx) => new COM_Trs_SalesQuotationFreightCharge
            {
                CompanyCode = entity.CompanyCode,
                QuotationNo = entity.QuotationNo,
                No = idx + 1,
                ChargeCode = c.ChargeCode,
                ChargeName = c.ChargeName,
                UnitPrice = c.UnitPrice,
                CurrencyCode = c.CurrencyCode
            }).ToList();

        entity.COM_Trs_SalesQuotationFreightCharges = newCharges;

        await qtmsContext.SaveChangesAsync();

        TempData["Success"] = "Quotation updated successfully.";
        return RedirectToAction(nameof(QuotationDetails), new { id = entity.QuotationNo });
    }

    [HttpGet]
    public async Task<IActionResult> QuotationDetails(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var entity = await qtmsContext.COM_Trs_SalesQuotations
            .Include(x => x.COM_Trs_SalesQuotationFreightCharges)
                .ThenInclude(c => c.COM_Trs_SalesQuotationFreightChargePriceTiers)
            .Include(x => x.COM_Trs_SalesQuotationShipmentDetails)
            .FirstOrDefaultAsync(x => x.QuotationNo == id);

        if (entity is null) return NotFound();

        await LoadDropdownsAsync();
        return View(entity);
    }

    [HttpGet]
    public async Task<IActionResult> PrintQuotation(string id)
    {
        return await QuotationDetails(id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviseQuotation(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        // Basic revision: duplicate quotation with new QuotationNo and increment revision number if tracked
        var src = await qtmsContext.COM_Trs_SalesQuotations
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
                No = c.No,
                ChargeCode = c.ChargeCode,
                ChargeName = c.ChargeName,
                UnitPrice = c.UnitPrice,
                CurrencyCode = c.CurrencyCode
            });
        }

        qtmsContext.COM_Trs_SalesQuotations.Add(copy);
        await qtmsContext.SaveChangesAsync();

        TempData["Success"] = "New revision created. You are now editing the revised quotation.";
        return RedirectToAction(nameof(EditQuotation), new { id = copy.QuotationNo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CopyQuotation(string id)
    {
        return await ReviseQuotation(id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeQuotationStatus(string id, string status)
    {
        if (string.IsNullOrWhiteSpace(status) || !AppLists.Statuses.Contains(status))
        {
            TempData["Error"] = "Invalid status.";
            return RedirectToAction(nameof(QuotationDetails), new { id });
        }

        var entity = await qtmsContext.COM_Trs_SalesQuotations.FirstOrDefaultAsync(x => x.QuotationNo == id);
        if (entity is null) return NotFound();
        entity.StatusName = status;
        await qtmsContext.SaveChangesAsync();

        TempData["Success"] = $"Quotation status changed to {status}.";
        return RedirectToAction(nameof(QuotationDetails), new { id = entity.QuotationNo });
    }

    private async Task LoadDropdownsAsync()
    {
        //var companies = await qtmsContext.COM_Trs_SalesQuotations
        //    .Where(x => !string.IsNullOrEmpty(x.CompanyCode))
        //    .Select(x => x.CompanyCode.Trim())
        //    .Distinct()
        //    .OrderBy(c => c)
        //    .ToListAsync();

        //var list = companies.Select(c => new CompanyLookupViewModel { CompanyCode = c, DisplayName = c }).ToList();
        //ViewBag.Companies = new SelectList(list, nameof(CompanyLookupViewModel.CompanyCode), nameof(CompanyLookupViewModel.DisplayName));
    }

    private static void NormalizeBeforeValidate(ref COM_Trs_SalesQuotation input)
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

        // Ensure collection exists
        input.COM_Trs_SalesQuotationFreightCharges ??= new List<COM_Trs_SalesQuotationFreightCharge>();

        // Remove "play" rows: rows with no meaningful data.
        // A row is considered empty when ChargeCode and ChargeName are blank and numeric fields are default/zero.
        var chgs = input.COM_Trs_SalesQuotationFreightCharges
            .Where(c => c != null)
            .Select(c =>
            {
                // Trim string fields to make checks consistent
                c.ChargeCode = c.ChargeCode?.Trim();
                c.ChargeName = c.ChargeName?.Trim();
                c.CurrencyCode = c.CurrencyCode?.Trim();
                return c;
            })
            .Where(c =>
                !string.IsNullOrWhiteSpace(c.ChargeCode) ||
                !string.IsNullOrWhiteSpace(c.ChargeName) ||
                (c.UnitPrice ?? 0) != 0 ||
                !string.IsNullOrWhiteSpace(c.CurrencyCode))
            .Select((c, index) =>
            {
                // Re-number lines sequentially
                c.No = index + 1;
                return c;
            })
            .ToList();

        input.COM_Trs_SalesQuotationFreightCharges = chgs;


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

    /// <summary>
    /// Centralized server-side validation for a quotation before persisting.
    /// Returns (isValid, message). The message should be user-friendly when isValid is false.
    /// The method contains try/catch as required.
    /// </summary>
    private static void ValidateQuotation(ref COM_Trs_SalesQuotation input)
    {

        var Invalidations = new List<string>();

        try
        {
            if (input == null)
            {
                throw new Exception("Invalid quotation data.");
            }

            NormalizeBeforeValidate(ref input);


            if (string.IsNullOrWhiteSpace(input.CompanyCode))
            {
                throw new Exception("Invalid Company.");
            }


        }
        catch (Exception ex)
        {
            Invalidations.Add($"{ex.Message}");
        }
        if (Invalidations.Count > 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Invalidations :-");
            foreach (var inValid in Invalidations)
            {
                sb.AppendLine($" •{inValid}");
            }
            throw new Exception(sb.ToString());
        }
    }

    private void SaveCore(ref COM_Trs_SalesQuotation entity, FormMode formMode)
    {
        try
        {
            var dateTimeNow = DateTime.Now; // AppLib.Utilities.DateTimeUtil.GetDateTimeNow(qtmsContext);
            var currentUser = "CurrentUser"; // Replace with actual user context

            // logs
            entity.UpdateDate = dateTimeNow;

            if (formMode == FormMode.Add || formMode == FormMode.Copy)
            {
                entity.CreateBy = currentUser;
                entity.UpdateDate = dateTimeNow;

            }

            // Enable distributed transactions so that multiple DB connections
            // (different servers / databases) can participate in the same TransactionScope.
            TransactionManager.ImplicitDistributedTransactions = true;

            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                if (formMode == FormMode.Add || formMode == FormMode.Copy)
                {
                    var runningNoResult = new OutputParameter<string>();
                    var returnValue = new OutputParameter<int>();

                    commonContext.Procedures
                        .COM_SP_SeriesNoGenerateAsync(
                             tableName: SeriesNo_TableName
                            , columnName: SeriesNo_ColumnName + entity.CompanyCode
                            , documentDate: entity.QuotationDate ?? dateTimeNow
                            , user: currentUser
                            , formatType: SeriesNo_FormatType
                            , runningNoResult: runningNoResult
                            , returnValue: returnValue)
                        .GetAwaiter()
                        .GetResult();

                    var newDocNo = runningNoResult.Value;

                    if (string.IsNullOrWhiteSpace(newDocNo))
                    {
                        throw new Exception("Unable to generate quotation number.");
                    }

                    entity.QuotationNo = newDocNo;

                    qtmsContext.COM_Trs_SalesQuotations.Add(entity);
                }
                else
                {

                    if (qtmsContext.Entry(entity).State == EntityState.Detached)
                    {
                        qtmsContext.COM_Trs_SalesQuotations.Update(entity);
                    }
                }

                qtmsContext.SaveChanges();

                if (commonContext.ChangeTracker.HasChanges())
                {
                    commonContext.SaveChanges();
                }

                scope.Complete();
            }

        }
        catch (Exception)
        {
            throw;
        }
    }


}