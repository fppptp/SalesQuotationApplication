using LMSModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SQTWeb.Services.Masters;

public sealed class AgentService : IAgentService
{
    private readonly IDbContextFactory<LMSContext> _dbFactory;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "MASTER:AGENT";

    public AgentService(
        IDbContextFactory<LMSContext> dbFactory,
        IMemoryCache cache)
    {
        _dbFactory = dbFactory;
        _cache = cache;
    }

    public async Task<IReadOnlyList<COM_View_Agent>> GetOptionsAsync(bool forceRefresh = false)
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

            var list = await db.COM_View_Agents
                .AsNoTracking()
                .Where(x => !string.IsNullOrWhiteSpace(x.custcode))
                .OrderBy(x => x.custname)
                .ToListAsync();

            return (IReadOnlyList<COM_View_Agent>)list;
        });

        return result ?? Array.Empty<COM_View_Agent>();
    }

    public Task ClearCacheAsync()
    {
        _cache.Remove(CacheKey);
        return Task.CompletedTask;
    }
}
