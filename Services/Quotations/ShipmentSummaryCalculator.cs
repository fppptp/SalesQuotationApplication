using QTMSModel.Models;

namespace SQTWeb.Services.Quotations;

public class ShipmentSummaryCalculator : IShipmentSummaryCalculator
{
    /// <inheritdoc />
    public void RecalculateSummary(COM_Trs_SalesQuotation quotation)
    {
        ArgumentNullException.ThrowIfNull(quotation);

        var details = quotation.COM_Trs_SalesQuotationShipmentDetails
            ?? Enumerable.Empty<COM_Trs_SalesQuotationShipmentDetail>();

        quotation.GrandTotalGrossWeightKg = details.Sum(d => d.TotalGrossWeightKG ?? 0);
        quotation.GrandTotalVolumeCBM = details.Sum(d => d.TotalVolumeCBM ?? 0);
    }
}
