using QTMSModel.Models;
using SQTWeb.Extensions;

namespace SQTWeb.Services.Quotations;

public class ShipmentDetailCalculator : IShipmentDetailCalculator
{
    /// <inheritdoc />
    public void RecalculateDetail(COM_Trs_SalesQuotationShipmentDetail detail)
    {
        ArgumentNullException.ThrowIfNull(detail);
        detail.DimensionCal();
    }
}
