using QTMSModel.Models;

namespace SQTWeb.Services.Quotations;

public class QuotationHeaderCalculator : IQuotationHeaderCalculator
{
    /// <inheritdoc />
    public void RecalculateHeader(COM_Trs_SalesQuotation quotation)
    {
        ArgumentNullException.ThrowIfNull(quotation);

        // Freight totals – convert each charge to the header currency
        decimal totalFreightDest = 0;
        foreach (var charge in quotation.COM_Trs_SalesQuotationFreightCharges
            ?? Enumerable.Empty<COM_Trs_SalesQuotationFreightCharge>())
        {
            var unitPrice = charge.UnitPrice ?? 0;
            var margin = charge.MarginAmount;
            var exRate = charge.ExchangeRateToHeader != 0 ? charge.ExchangeRateToHeader : 1m;
            totalFreightDest += (unitPrice + margin) * exRate;
        }

        quotation.GrandTotalFrieghtAmountDest = totalFreightDest;
    }
}
