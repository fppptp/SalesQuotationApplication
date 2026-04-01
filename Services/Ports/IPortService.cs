using LMSModel.Models;
namespace SQTWeb.Services.Ports;

public interface IPortService
{
    Task<IReadOnlyList<COM_View_Port>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}
