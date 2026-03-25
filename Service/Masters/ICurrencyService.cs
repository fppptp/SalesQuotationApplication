using SQTWeb.DTOModels;
namespace SQTWeb.Services.Masters;

public interface ICurrencyService
{
    Task<IReadOnlyList<SelectOptionDto>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}