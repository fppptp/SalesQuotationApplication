using QTMSModel.Models;

namespace SQTWeb.Services.Quotations;

public interface IShipmentSummaryCalculator
{
    /// <summary>
    /// Aggregates all shipment detail rows and writes
    /// <see cref="COM_Trs_SalesQuotation.GrandTotalGrossWeightKg"/> and
    /// <see cref="COM_Trs_SalesQuotation.GrandTotalVolumeCBM"/> directly on the entity.
    /// </summary>
    void RecalculateSummary(COM_Trs_SalesQuotation quotation);
}
