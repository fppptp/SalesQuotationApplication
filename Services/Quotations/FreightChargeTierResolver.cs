using QTMSModel.Models;

namespace SQTWeb.Services.Quotations;

public class FreightChargeTierResolver : IFreightChargeTierResolver
{
    /// <inheritdoc />
    public COM_Trs_SalesQuotationFreightChargePriceTier? ResolveMatchedTier(
        COM_Trs_SalesQuotationFreightCharge charge,
        COM_Trs_SalesQuotation quotation)
    {
        ArgumentNullException.ThrowIfNull(charge);
        ArgumentNullException.ThrowIfNull(quotation);

        var tiers = charge.COM_Trs_SalesQuotationFreightChargePriceTiers;
        if (tiers == null || !tiers.Any())
            return null;

        // Determine the lookup value based on ChargeUnit
        var unit = charge.ChargeUnit?.Trim().ToUpperInvariant() ?? string.Empty;
        decimal lookupValue = unit switch
        {
            "KG" or "KGS" => quotation.GrandTotalGrossWeightKg ?? 0,
            _ => quotation.GrandTotalVolumeCBM ?? 0
        };

        return tiers
            .Where(t => (t.FromValue ?? 0) <= lookupValue
                     && lookupValue <= (t.ToValue ?? decimal.MaxValue))
            .OrderByDescending(t => t.FromValue)
            .FirstOrDefault();
    }
}
