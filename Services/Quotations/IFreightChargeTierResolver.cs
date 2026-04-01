using QTMSModel.Models;

namespace SQTWeb.Services.Quotations;

public interface IFreightChargeTierResolver
{
    /// <summary>
    /// Finds the price tier whose <c>FromValue..ToValue</c> range contains the relevant
    /// measurement (CBM or weight) from the quotation header totals.
    /// Returns <c>null</c> when no tier matches or no tiers exist.
    /// </summary>
    COM_Trs_SalesQuotationFreightChargePriceTier? ResolveMatchedTier(
        COM_Trs_SalesQuotationFreightCharge charge,
        COM_Trs_SalesQuotation quotation);
}
