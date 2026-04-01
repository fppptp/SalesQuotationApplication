using QTMSModel.Models;

namespace SQTWeb.Services.Quotations;

public interface IFreightChargeCalculator
{
    /// <summary>
    /// Re-evaluates every freight charge against the quotation's shipment totals.
    /// When a matching price tier is found its <c>UnitPrice</c> is applied to the charge.
    /// </summary>
    void RecalculateAllCharges(COM_Trs_SalesQuotation quotation);
}
