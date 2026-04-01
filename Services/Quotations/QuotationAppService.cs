using AppLib;
using AppLib.DocumentControl;
using SQTWeb.Navigation;
using COMMONModel.Models;
using Microsoft.EntityFrameworkCore;
using QTMSModel.Models;
using SQTWeb.Extensions;
using System.Text;
using System.Transactions;
using UtilityLib.Extensions;
using SQTWeb.Services.QuotationStatus;

namespace SQTWeb.Services.Quotations;

public class QuotationAppService : IQuotationAppService
{
    private readonly QTMSContext _qtmsContext;
    private readonly COMMONContext _commonContext;
    private readonly IQuotationStatusService _statusService;
    private readonly IQuotationCalculationOrchestrator _calculationOrchestrator;

    private string SeriesNo_TableName => _qtmsContext.COM_Trs_SalesQuotations.GetEFCoreTableName();
    private string SeriesNo_ColumnName => _qtmsContext.COM_Trs_SalesQuotations.GetEFCoreColumnName(x => x.QuotationNo);
    private string SeriesNo_FormatType => SeriesNo.Format.PrefixY2_M2_No.ToString();

    public QuotationAppService(
        QTMSContext qtmsContext,
        COMMONContext commonContext,
        IQuotationStatusService statusService,
        IQuotationCalculationOrchestrator calculationOrchestrator)
    {
        _qtmsContext = qtmsContext;
        _commonContext = commonContext;
        _statusService = statusService;
        _calculationOrchestrator = calculationOrchestrator;
    }

    /// <inheritdoc />
    public async Task SaveAsync(COM_Trs_SalesQuotation entity, FormMode formMode)
    {
        NormalizeQuotation(entity);
        _calculationOrchestrator.RecalculateAll(entity);
        ValidateQuotation(entity);
        await SaveCoreAsync(entity, formMode);
    }

    /// <inheritdoc />
    public void CalculateHeaderDimension(COM_Trs_SalesQuotation entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _calculationOrchestrator.RecalculateAll(entity);
    }

    /// <inheritdoc />
    public async Task ChangeStatusAsync(string quotationNo, string status)
    {
        var validStatuses = await _statusService.GetOptionsAsync();
        if (string.IsNullOrWhiteSpace(status) || !validStatuses.Any(s => string.Equals(s.StatusDescription, status, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception("Invalid status.");
        }

        var entity = await _qtmsContext.COM_Trs_SalesQuotations.FirstOrDefaultAsync(x => x.QuotationNo == quotationNo)
            ?? throw new Exception("Quotation not found.");

        entity.StatusName = status;
        await _qtmsContext.SaveChangesAsync();
    }

    // ───────────────────────── Private helpers ─────────────────────────

    /// <summary>
    /// Cleans up null / blank rows, re-numbers child collections, and trims string fields.
    /// </summary>
    private static void NormalizeQuotation(COM_Trs_SalesQuotation entity)
    {
        // Ensure Shipment details
        entity.COM_Trs_SalesQuotationShipmentDetails ??= new List<COM_Trs_SalesQuotationShipmentDetail>();
        var blankShipments = entity.COM_Trs_SalesQuotationShipmentDetails.Where(s => s.Quantity == 0 && s.GrossWeight == null && s.Length == null && s.Width == null && s.Height == null).ToList();
        foreach (var s in blankShipments) { entity.COM_Trs_SalesQuotationShipmentDetails.Remove(s); }
        var shipmentIndex = 0;
        foreach (var shpt in entity.COM_Trs_SalesQuotationShipmentDetails)
        {
            shipmentIndex += 1;
            shpt.No = shipmentIndex;
        }

        // Ensure collection exists
        entity.COM_Trs_SalesQuotationFreightCharges ??= new List<COM_Trs_SalesQuotationFreightCharge>();
        var nullChgs = entity.COM_Trs_SalesQuotationFreightCharges.Where(c => c == null).ToList();
        foreach (var c in nullChgs) { entity.COM_Trs_SalesQuotationFreightCharges.Remove(c); }
        var chgId = 0;
        foreach (var chg in entity.COM_Trs_SalesQuotationFreightCharges)
        {
            chgId += 1;
            chg.No = chgId;
            // Default ExchangeRateToHeader to 1 when zero (empty form field)
            if (chg.ExchangeRateToHeader == 0) chg.ExchangeRateToHeader = 1m;
            // Ensure price tiers
            chg.COM_Trs_SalesQuotationFreightChargePriceTiers ??= new List<COM_Trs_SalesQuotationFreightChargePriceTier>();
            var nullTiers = chg.COM_Trs_SalesQuotationFreightChargePriceTiers.Where(t => t == null).ToList();
            foreach (var t in nullTiers) { chg.COM_Trs_SalesQuotationFreightChargePriceTiers.Remove(t); }
            var tierId = 0;
            foreach (var tier in chg.COM_Trs_SalesQuotationFreightChargePriceTiers)
            {
                tierId += 1;
                tier.No = tierId;
            }
        }

        // Remove "play" rows: rows with no meaningful data.
        // A row is considered empty when ChargeCode and ChargeName are blank and numeric fields are default/zero.
        var chgs = entity.COM_Trs_SalesQuotationFreightCharges
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

        entity.COM_Trs_SalesQuotationFreightCharges = chgs;
    }

    /// <summary>
    /// Centralized server-side validation for a quotation before persisting.
    /// Throws an <see cref="Exception"/> with a user-friendly message when validation fails.
    /// </summary>
    private static void ValidateQuotation(COM_Trs_SalesQuotation entity)
    {
        var invalidations = new List<string>();

        if (entity == null)
        {
            invalidations.Add("Invalid quotation data.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(entity.CompanyCode))
                invalidations.Add("Company is required.");

            if (entity.QuotationDate == null)
                invalidations.Add("Quotation Date is required.");

            if (string.IsNullOrWhiteSpace(entity.CurrencyCode))
                invalidations.Add("Currency is required.");

            if (entity.ExchangeRate <= 0)
                invalidations.Add("Exchange Rate must be greater than zero.");

            // Validate freight charges
            if (entity.COM_Trs_SalesQuotationFreightCharges != null)
            {
                var chgNo = 0;
                foreach (var chg in entity.COM_Trs_SalesQuotationFreightCharges)
                {
                    chgNo++;
                    if (string.IsNullOrWhiteSpace(chg.ChargeCode))
                        invalidations.Add($"Freight Charge #{chgNo}: Charge Code is required.");

                    if (string.IsNullOrWhiteSpace(chg.CurrencyCode))
                        invalidations.Add($"Freight Charge #{chgNo}: Currency is required.");

                    if (chg.ExchangeRateToHeader <= 0)
                        invalidations.Add($"Freight Charge #{chgNo}: Exchange Rate must be greater than zero.");
                }
            }

            // Validate shipment details
            if (entity.COM_Trs_SalesQuotationShipmentDetails != null)
            {
                var shipNo = 0;
                foreach (var shpt in entity.COM_Trs_SalesQuotationShipmentDetails)
                {
                    shipNo++;
                    if (string.IsNullOrWhiteSpace(shpt.PackageUnit))
                        invalidations.Add($"Shipment #{shipNo}: Package Unit is required.");
                }
            }
        }

        if (invalidations.Count > 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Invalidations :-");
            foreach (var inValid in invalidations)
            {
                sb.AppendLine($" •{inValid}");
            }
            throw new Exception(sb.ToString());
        }
    }

    /// <summary>
    /// Persists the quotation to the database inside a distributed transaction.
    /// Generates a new quotation number for Add/Copy modes.
    /// </summary>
    private async Task SaveCoreAsync(COM_Trs_SalesQuotation entity, FormMode formMode)
    {
        var dateTimeNow = DateTime.Now;

        // Audit fields
        entity.UpdateDate = dateTimeNow;
        entity.UpdateBy = AppLib.Authication.User.Username;

        if (formMode == FormMode.Add || formMode == FormMode.Copy)
        {
            entity.CreateBy = AppLib.Authication.User.Username;
            entity.UpdateDate = dateTimeNow;
        }

        // Enable distributed transactions so that multiple DB connections
        // (different servers / databases) can participate in the same TransactionScope.
        TransactionManager.ImplicitDistributedTransactions = true;

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        if (formMode == FormMode.Add || formMode == FormMode.Copy)
        {
            var runningNoResult = new OutputParameter<string>();
            var returnValue = new OutputParameter<int>();

            await _commonContext.Procedures
                .COM_SP_SeriesNoGenerateAsync(
                     tableName: SeriesNo_TableName,
                     columnName: SeriesNo_ColumnName + entity.CompanyCode,
                     documentDate: entity.QuotationDate ?? dateTimeNow,
                     user: AppLib.Authication.User.Username,
                     formatType: SeriesNo_FormatType,
                     runningNoResult: runningNoResult,
                     returnValue: returnValue);

            var newDocNo = runningNoResult.Value;

            if (string.IsNullOrWhiteSpace(newDocNo))
            {
                throw new Exception("Unable to generate quotation number.");
            }

            entity.QuotationNo = newDocNo;
        }

        // Propagate composite keys to all child entities
        foreach (var shpt in entity.COM_Trs_SalesQuotationShipmentDetails)
        {
            shpt.CompanyCode = entity.CompanyCode;
            shpt.QuotationNo = entity.QuotationNo;
        }
        foreach (var chg in entity.COM_Trs_SalesQuotationFreightCharges)
        {
            chg.CompanyCode = entity.CompanyCode;
            chg.QuotationNo = entity.QuotationNo;
            foreach (var tier in chg.COM_Trs_SalesQuotationFreightChargePriceTiers)
            {
                tier.CompanyCode = entity.CompanyCode;
                tier.QuotationNo = entity.QuotationNo;
                tier.ChargeNo = chg.No;
            }
        }

        if (formMode == FormMode.Add || formMode == FormMode.Copy)
        {
            _qtmsContext.COM_Trs_SalesQuotations.Add(entity);
        }
        else
        {
            var companyCode = entity.CompanyCode;
            var quotationNo = entity.QuotationNo;

            // Load only the header (tracked) for field-level update
            var existing = _qtmsContext.COM_Trs_SalesQuotations
                .FirstOrDefault(x => x.CompanyCode == companyCode && x.QuotationNo == quotationNo)
                ?? throw new Exception("Quotation not found.");

            _qtmsContext.Entry(existing).CurrentValues.SetValues(entity);

            // Delete existing children from DB (deepest → shallowest)
            _qtmsContext.COM_Trs_SalesQuotationFreightChargePriceTiers
                .Where(x => x.CompanyCode == companyCode && x.QuotationNo == quotationNo)
                .ExecuteDelete();
            _qtmsContext.COM_Trs_SalesQuotationFreightCharges
                .Where(x => x.CompanyCode == companyCode && x.QuotationNo == quotationNo)
                .ExecuteDelete();
            _qtmsContext.COM_Trs_SalesQuotationShipmentDetails
                .Where(x => x.CompanyCode == companyCode && x.QuotationNo == quotationNo)
                .ExecuteDelete();

            // Re-add children from posted model (keys already propagated above)
            _qtmsContext.COM_Trs_SalesQuotationShipmentDetails.AddRange(entity.COM_Trs_SalesQuotationShipmentDetails);
            _qtmsContext.COM_Trs_SalesQuotationFreightCharges.AddRange(entity.COM_Trs_SalesQuotationFreightCharges);
        }

        await _qtmsContext.SaveChangesAsync();

        if (_commonContext.ChangeTracker.HasChanges())
        {
            await _commonContext.SaveChangesAsync();
        }

        scope.Complete();
    }
}
