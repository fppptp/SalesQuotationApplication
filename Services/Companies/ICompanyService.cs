using FMSModel.Models;

namespace SQTWeb.Services.Companies;

public interface ICompanyService
{
    Task<IReadOnlyList<COM_MS_Company>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}
