using COMMONModel.Models;
namespace SQTWeb.Services.Masters;

public interface IQuotationStatusService
{
    Task<IReadOnlyList<COM_Ms_Status>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}
