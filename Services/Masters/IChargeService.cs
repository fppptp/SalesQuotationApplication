using QTMSModel.Models;
namespace SQTWeb.Services.Masters;

public interface IChargeService
{
    Task<IReadOnlyList<COM_Ms_SalesQuotationCharge>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}
