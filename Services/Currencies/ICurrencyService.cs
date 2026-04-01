using FMSModel.Models;

namespace SQTWeb.Services.Currencies;

public interface ICurrencyService
{
    Task<IReadOnlyList<COM_View_MS_TISI_Currency>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}
