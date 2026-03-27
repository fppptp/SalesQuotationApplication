using QTMSModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SQTWeb.Services.Masters;

public sealed class UnitOfMeasureService : IUnitOfMeasureService
{
    private readonly IDbContextFactory<QTMSContext> _dbFactory;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "MASTER:UNIT_OF_MEASURE";

    public UnitOfMeasureService(
        IDbContextFactory<QTMSContext> dbFactory,
        IMemoryCache cache)
    {
        _dbFactory = dbFactory;
        _cache = cache;
    }

    public async Task<IReadOnlyList<COM_Ms_UnitOfMeasure>> GetOptionsAsync(bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            _cache.Remove(CacheKey);
        }

        var result = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12);
            entry.SlidingExpiration = TimeSpan.FromHours(2);

            await using var db = await _dbFactory.CreateDbContextAsync();

            var list = await db.COM_Ms_UnitOfMeasures
                .AsNoTracking()
                .Where(x => !string.IsNullOrWhiteSpace(x.UnitCode))
                .OrderBy(x => x.UnitCode)
                .ToListAsync();

            return (IReadOnlyList<COM_Ms_UnitOfMeasure>)list;
        });

        return result ?? Array.Empty<COM_Ms_UnitOfMeasure>();
    }

    public Task ClearCacheAsync()
    {
        _cache.Remove(CacheKey);
        return Task.CompletedTask;
    }
}
