using UOMLib.Domain.Interfaces;
using UOMLib.Models;

namespace SQTWeb.Services.UnitOfMeasure;

public sealed class UnitOfMeasureService : IUnitOfMeasureService
{
    private readonly IUomCacheService _uomCache;

    public UnitOfMeasureService(IUomCacheService uomCache)
    {
        _uomCache = uomCache;
    }

    public async Task<IReadOnlyList<COM_Ms_UnitOfMeasure>> GetOptionsAsync(bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            await _uomCache.RefreshAsync();
        }

        return await _uomCache.GetUnitsAsync();
    }

    public Task ClearCacheAsync()
    {
        return _uomCache.RefreshAsync();
    }
}
