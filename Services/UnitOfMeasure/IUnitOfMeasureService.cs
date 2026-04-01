using UOMLib.Models;
namespace SQTWeb.Services.UnitOfMeasure;

public interface IUnitOfMeasureService
{
    Task<IReadOnlyList<COM_Ms_UnitOfMeasure>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}
