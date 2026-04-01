using COMMONModel.Models;
namespace SQTWeb.Services.QuotationStatus;

public interface IQuotationStatusService
{
    Task<IReadOnlyList<COM_Ms_Status>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}
