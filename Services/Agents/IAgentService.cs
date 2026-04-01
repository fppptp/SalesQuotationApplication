using LMSModel.Models;
namespace SQTWeb.Services.Agents;

public interface IAgentService
{
    Task<IReadOnlyList<COM_View_Agent>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}
