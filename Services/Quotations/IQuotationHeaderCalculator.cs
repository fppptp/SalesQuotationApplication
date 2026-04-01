using QTMSModel.Models;

namespace SQTWeb.Services.Quotations;

public interface IQuotationHeaderCalculator
{
    /// <summary>
    /// Computes freight totals from the quotation's charges and writes them to the header.
    /// Shipment summaries must already be set on the entity before calling this method.
    /// </summary>
    void RecalculateHeader(COM_Trs_SalesQuotation quotation);
}
