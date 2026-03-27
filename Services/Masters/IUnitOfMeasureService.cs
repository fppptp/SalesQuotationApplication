using QTMSModel.Models;
namespace SQTWeb.Services.Masters;

public interface IUnitOfMeasureService
{
    Task<IReadOnlyList<COM_Ms_UnitOfMeasure>> GetOptionsAsync(bool forceRefresh = false);
    Task ClearCacheAsync();
}
