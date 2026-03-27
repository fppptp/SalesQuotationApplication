using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using FMSModel.Models;
using ACMSModel.Models;

namespace SQTWeb.Cache;

public static class CompanyCache
{
    // Simple thread-safe cache for Company list
    private static readonly object _lock = new();
    private static List<COM_MS_Company> _companies = new();

    public static IReadOnlyList<COM_MS_Company> Companies => _companies;

    public static async Task RefreshAsync(FMSContext ctx)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));
        var list = await ctx.COM_MS_Companies.AsNoTracking().OrderBy(c => c.CompanyCode).ToListAsync();
        lock (_lock)
        {
            _companies = list;
        }
    }

    public static COM_MS_Company? Find(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        return _companies.FirstOrDefault(c => string.Equals(c.CompanyCode?.Trim(), code.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
