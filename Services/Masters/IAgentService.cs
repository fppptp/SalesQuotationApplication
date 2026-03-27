using LMSModel.Models;
namespace SQTWeb.Services.Masters;

public interface IAgentService
{
    Task<IReadOnlyList<COM_View_Agent>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}
