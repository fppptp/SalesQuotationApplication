using COMMONModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SQTWeb.Services.Masters;

public sealed class QuotationStatusService : IQuotationStatusService
{
    private readonly IDbContextFactory<COMMONContext> _dbFactory;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "MASTER:QUOTATION_STATUS";

    public QuotationStatusService(
        IDbContextFactory<COMMONContext> dbFactory,
        IMemoryCache cache)
    {
        _dbFactory = dbFactory;
        _cache = cache;
    }

    public async Task<IReadOnlyList<COM_Ms_Status>> GetOptionsAsync(bool forceRefresh = false)
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

            var list = await db.COM_Ms_Statuses
                .AsNoTracking()
                .Where(x => x.StatusDocument == "SalesQuotation")
                .OrderBy(x => x.StatusNo)
                .ToListAsync();

            return (IReadOnlyList<COM_Ms_Status>)list;
        });

        return result ?? Array.Empty<COM_Ms_Status>();
    }

    public Task ClearCacheAsync()
    {
        _cache.Remove(CacheKey);
        return Task.CompletedTask;
    }
}
