using LMSModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SQTWeb.Services.Masters;

public sealed class PortService : IPortService
{
    private readonly IDbContextFactory<LMSContext> _dbFactory;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "MASTER:PORT";

    public PortService(
        IDbContextFactory<LMSContext> dbFactory,
        IMemoryCache cache)
    {
        _dbFactory = dbFactory;
        _cache = cache;
    }

    public async Task<IReadOnlyList<COM_View_Port>> GetOptionsAsync(bool forceRefresh = false)
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

            var list = await db.COM_View_Ports
                .AsNoTracking()
                .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                .OrderBy(x => x.Name)
                .ToListAsync();

            return (IReadOnlyList<COM_View_Port>)list;
        });

        return result ?? Array.Empty<COM_View_Port>();
    }

    public Task ClearCacheAsync()
    {
        _cache.Remove(CacheKey);
        return Task.CompletedTask;
    }
}
