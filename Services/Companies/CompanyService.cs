using FMSModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SQTWeb.Services.Companies;

public sealed class CompanyService : ICompanyService
{
    private readonly IDbContextFactory<FMSContext> _dbFactory;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "MASTER:COMPANY";

    public CompanyService(
        IDbContextFactory<FMSContext> dbFactory,
        IMemoryCache cache)
    {
        _dbFactory = dbFactory;
        _cache = cache;
    }

    public async Task<IReadOnlyList<COM_MS_Company>> GetOptionsAsync(bool forceRefresh = false)
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

            var list = await db.COM_MS_Companies
                .AsNoTracking()
                .Where(x => !string.IsNullOrWhiteSpace(x.CompanyCode))
                .OrderBy(x => x.CompanyCode)
                .ToListAsync();

            return (IReadOnlyList<COM_MS_Company>)list;
        });

        return result ?? Array.Empty<COM_MS_Company>();
    }

    public Task ClearCacheAsync()
    {
        _cache.Remove(CacheKey);
        return Task.CompletedTask;
    }
}
