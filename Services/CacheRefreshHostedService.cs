using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SQTWeb.Services.Agents;
using SQTWeb.Services.Ports;
using SQTWeb.Services.QuotationStatus;
using SQTWeb.Services.UnitOfMeasure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SQTWeb.Services
{
    public class CacheRefreshHostedService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<CacheRefreshHostedService> _logger;
        private readonly int _intervalMinutes;

        public CacheRefreshHostedService(IServiceProvider provider, ILogger<CacheRefreshHostedService> logger, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _provider = provider;
            _logger = logger;
            _intervalMinutes = 60; // default
            try
            {
                var v = configuration["CompanyCache:RefreshIntervalMinutes"];
                if (!string.IsNullOrWhiteSpace(v) && int.TryParse(v, out var parsed)) _intervalMinutes = parsed;
            }
            catch {
                // ignore and use default
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cache refresh hosted service started. First refresh in {minutes} minutes.", _intervalMinutes);

            // Skip initial run — caches are preloaded synchronously in Program.cs
            await Task.Delay(TimeSpan.FromMinutes(Math.Max(1, _intervalMinutes)), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _provider.CreateScope();
                    var sp = scope.ServiceProvider;

                    await RefreshServiceAsync<ICompanyService>(sp, "Company");
                    await RefreshServiceAsync<ICurrencyService>(sp, "Currency");
                    await RefreshServiceAsync<IQuotationStatusService>(sp, "QuotationStatus");
                    await RefreshServiceAsync<IChargeService>(sp, "Charge");
                    await RefreshServiceAsync<IUnitOfMeasureService>(sp, "UnitOfMeasure");
                    await RefreshServiceAsync<IAgentService>(sp, "Agent");
                    await RefreshServiceAsync<IPortService>(sp, "Port");

                    _logger.LogInformation("All master caches refreshed at {time}.", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing master caches.");
                }

                await Task.Delay(TimeSpan.FromMinutes(Math.Max(1, _intervalMinutes)), stoppingToken);
            }

            _logger.LogInformation("Cache refresh hosted service stopping.");
        }

        private async Task RefreshServiceAsync<T>(IServiceProvider sp, string name) where T : class
        {
            try
            {
                var svc = sp.GetService<T>();
                if (svc != null)
                {
                    // All master services expose GetOptionsAsync(bool forceRefresh) via their interface
                    var method = typeof(T).GetMethod("GetOptionsAsync");
                    if (method != null)
                    {
                        var task = (Task?)method.Invoke(svc, [true]);
                        if (task != null) await task;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh {CacheName} cache.", name);
            }
        }
    }
}
