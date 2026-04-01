using QTMSModel.Models;

namespace SQTWeb.Services.Quotations;

public class QuotationCalculationOrchestrator : IQuotationCalculationOrchestrator
{
    private readonly IShipmentDetailCalculator _shipmentDetailCalculator;
    private readonly IShipmentSummaryCalculator _shipmentSummaryCalculator;
    private readonly IFreightChargeCalculator _freightChargeCalculator;
    private readonly IQuotationHeaderCalculator _quotationHeaderCalculator;

    public QuotationCalculationOrchestrator(
        IShipmentDetailCalculator shipmentDetailCalculator,
        IShipmentSummaryCalculator shipmentSummaryCalculator,
        IFreightChargeCalculator freightChargeCalculator,
        IQuotationHeaderCalculator quotationHeaderCalculator)
    {
        _shipmentDetailCalculator = shipmentDetailCalculator;
        _shipmentSummaryCalculator = shipmentSummaryCalculator;
        _freightChargeCalculator = freightChargeCalculator;
        _quotationHeaderCalculator = quotationHeaderCalculator;
    }

    /// <inheritdoc />
    public void RecalculateAfterShipmentDetailChanged(
        COM_Trs_SalesQuotation quotation,
        COM_Trs_SalesQuotationShipmentDetail changedDetail)
    {
        ArgumentNullException.ThrowIfNull(quotation);
        ArgumentNullException.ThrowIfNull(changedDetail);

        // 1. Recalculate the changed shipment detail row
        _shipmentDetailCalculator.RecalculateDetail(changedDetail);

        // 2. Write shipment summary directly onto the entity
        _shipmentSummaryCalculator.RecalculateSummary(quotation);

        // 3. Recalculate freight charges using entity totals
        _freightChargeCalculator.RecalculateAllCharges(quotation);

        // 4. Recalculate header freight totals
        _quotationHeaderCalculator.RecalculateHeader(quotation);
    }

    /// <inheritdoc />
    public void RecalculateAll(COM_Trs_SalesQuotation quotation)
    {
        ArgumentNullException.ThrowIfNull(quotation);

        // 1. Recalculate every shipment detail row
        foreach (var detail in quotation.COM_Trs_SalesQuotationShipmentDetails
            ?? Enumerable.Empty<COM_Trs_SalesQuotationShipmentDetail>())
        {
            _shipmentDetailCalculator.RecalculateDetail(detail);
        }

        // 2. Write shipment summary directly onto the entity
        _shipmentSummaryCalculator.RecalculateSummary(quotation);

        // 3. Recalculate freight charges using entity totals
        _freightChargeCalculator.RecalculateAllCharges(quotation);

        // 4. Recalculate header freight totals
        _quotationHeaderCalculator.RecalculateHeader(quotation);
    }
}
