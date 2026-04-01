using QTMSModel.Models;

namespace SQTWeb.Services.Quotations;

public interface IShipmentDetailCalculator
{
    /// <summary>
    /// Recalculates <see cref="COM_Trs_SalesQuotationShipmentDetail.TotalVolumeCBM"/>
    /// and <see cref="COM_Trs_SalesQuotationShipmentDetail.TotalGrossWeightKG"/> for a single row.
    /// </summary>
    void RecalculateDetail(COM_Trs_SalesQuotationShipmentDetail detail);
}
