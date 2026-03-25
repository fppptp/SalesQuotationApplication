using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace SQTWeb.Models.FMS;

public static class CurrencyCache
{
    // Simple thread-safe cache for currency list
    private static readonly object _lock = new();
    private static List<COM_View_MS_TISI_Currency> _currencies = new();

    public static IReadOnlyList<COM_View_MS_TISI_Currency> Currencies => _currencies;

    public static async Task RefreshAsync(FMSContext ctx)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));
        var list = await ctx.COM_View_MS_TISI_Currencies.AsNoTracking().OrderBy(c => c.CurrencyCode).ToListAsync();
        lock (_lock)
        {
            _currencies = list;
        }
    }

    public static COM_View_MS_TISI_Currency? Find(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        return _currencies.FirstOrDefault(c => string.Equals(c.CurrencyCode?.Trim(), code.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
