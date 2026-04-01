using SQTWeb.Navigation;
using QTMSModel.Models;

namespace SQTWeb.Services.Quotations;

public interface IQuotationAppService
{
    /// <summary>
    /// Orchestrates the full save flow: Normalize → Validate → Persist.
    /// Throws an <see cref="Exception"/> with a user-friendly message when validation fails.
    /// </summary>
    Task SaveAsync(COM_Trs_SalesQuotation entity, FormMode formMode);

    /// <summary>
    /// Calculates dimension totals on the header from its shipment details (in-memory only).
    /// </summary>
    void CalculateHeaderDimension(COM_Trs_SalesQuotation entity);

    /// <summary>
    /// Changes the status of an existing quotation after validating the target status.
    /// Throws an <see cref="Exception"/> when the status is invalid or the quotation is not found.
    /// </summary>
    Task ChangeStatusAsync(string quotationNo, string status);
}
