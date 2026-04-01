using QTMSModel.Models;

namespace SQTWeb.Services.Quotations;

public interface IQuotationCalculationOrchestrator
{
    /// <summary>
    /// Recalculates a single changed shipment-detail row, then cascades through
    /// shipment summary → freight charges → header totals.
    /// </summary>
    void RecalculateAfterShipmentDetailChanged(
        COM_Trs_SalesQuotation quotation,
        COM_Trs_SalesQuotationShipmentDetail changedDetail);

    /// <summary>
    /// Full recalculation of every shipment detail, freight charge, and header total.
    /// Always called during the save flow so server values are authoritative.
    /// </summary>
    void RecalculateAll(COM_Trs_SalesQuotation quotation);
}
