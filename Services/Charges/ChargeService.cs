using QTMSModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SQTWeb.Services.Charges;

public sealed class ChargeService : IChargeService
{
    private readonly IDbContextFactory<QTMSContext> _dbFactory;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "MASTER:CHARGE";

    public ChargeService(
        IDbContextFactory<QTMSContext> dbFactory,
        IMemoryCache cache)
    {
        _dbFactory = dbFactory;
        _cache = cache;
    }

    public async Task<IReadOnlyList<COM_Ms_SalesQuotationCharge>> GetOptionsAsync(bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            _cache.Remove(CacheKey);
        }

        var result = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            entry.SlidingExpiration = TimeSpan.FromMinutes(30);

            await using var db = await _dbFactory.CreateDbContextAsync();

            var list = await db.COM_Ms_SalesQuotationCharges
                .AsNoTracking()
                .Where(x => x.Void != true)
                .Where(x => !string.IsNullOrWhiteSpace(x.ChargeCode))
                .OrderBy(x => x.ChargeCode)
                .ToListAsync();

            return (IReadOnlyList<COM_Ms_SalesQuotationCharge>)list;
        });

        return result ?? Array.Empty<COM_Ms_SalesQuotationCharge>();
    }

    public Task ClearCacheAsync()
    {
        _cache.Remove(CacheKey);
        return Task.CompletedTask;
    }
}
