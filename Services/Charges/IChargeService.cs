using QTMSModel.Models;

namespace SQTWeb.Services.Charges;

public interface IChargeService
{
    Task<IReadOnlyList<COM_Ms_SalesQuotationCharge>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}
