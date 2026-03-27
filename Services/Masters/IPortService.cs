using LMSModel.Models;
namespace SQTWeb.Services.Masters;

public interface IPortService
{
    Task<IReadOnlyList<COM_View_Port>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}
