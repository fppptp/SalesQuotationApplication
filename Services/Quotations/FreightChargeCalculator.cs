using QTMSModel.Models;

namespace SQTWeb.Services.Quotations;

public class FreightChargeCalculator : IFreightChargeCalculator
{
    private readonly IFreightChargeTierResolver _tierResolver;

    public FreightChargeCalculator(IFreightChargeTierResolver tierResolver)
    {
        _tierResolver = tierResolver;
    }

    /// <inheritdoc />
    public void RecalculateAllCharges(COM_Trs_SalesQuotation quotation)
    {
        ArgumentNullException.ThrowIfNull(quotation);
        if (quotation.COM_Trs_SalesQuotationFreightCharges == null) return;

        foreach (var charge in quotation.COM_Trs_SalesQuotationFreightCharges)
        {
            var matchedTier = _tierResolver.ResolveMatchedTier(charge, quotation);
            if (matchedTier != null)
            {
                charge.UnitPrice = matchedTier.UnitPrice;
            }
        }
    }
}
