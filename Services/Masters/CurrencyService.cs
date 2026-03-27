using FMSModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SQTWeb.Services.Masters;

public sealed class CurrencyService : ICurrencyService
{
    private readonly IDbContextFactory<FMSContext> _dbFactory;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "MASTER:CURRENCY";

    public CurrencyService(
        IDbContextFactory<FMSContext> dbFactory,
        IMemoryCache cache)
    {
        _dbFactory = dbFactory;
        _cache = cache;
    }

    public async Task<IReadOnlyList<COM_View_MS_TISI_Currency>> GetOptionsAsync(bool forceRefresh = false)
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

            var list = await db.COM_View_MS_TISI_Currencies
                .AsNoTracking()
                .Where(x => !string.IsNullOrWhiteSpace(x.CurrencyCode))
                .OrderBy(x => x.CurrencyCode)
                .ToListAsync();

            return (IReadOnlyList<COM_View_MS_TISI_Currency>)list;
        });

        return result ?? Array.Empty<COM_View_MS_TISI_Currency>();
    }

    public Task ClearCacheAsync()
    {
        _cache.Remove(CacheKey);
        return Task.CompletedTask;
    }
}
