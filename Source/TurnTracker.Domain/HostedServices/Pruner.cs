using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TurnTracker.Data;
using TurnTracker.Domain.Configuration;

namespace TurnTracker.Domain.HostedServices
{
    public class Pruner : ScopedBackgroundService
    {
        private readonly ILogger<Pruner> _logger;
        private readonly IOptions<AppSettings> _appSettings;

        public Pruner(IServiceProvider serviceProvider, ILogger<Pruner> logger, IOptions<AppSettings> appSettings) : base(serviceProvider)
        {
            _logger = logger;
            _appSettings = appSettings;
        }

        protected override async Task<TimeSpan> ExecuteScopedAsync(IServiceProvider services, CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Pruning");
                var now = DateTimeOffset.Now;
                var db = services.GetRequiredService<TurnContext>();
                
                await PruneExpiredLogins(db, now, stoppingToken);
                await PruneInactiveDeviceAuthorizations(db, now, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to prune");
            }

            return _appSettings.Value.PrunePeriod;
        }

        private async Task PruneExpiredLogins(TurnContext db, DateTimeOffset now, CancellationToken stoppingToken)
        {
            try
            {
                stoppingToken.ThrowIfCancellationRequested();
                _logger.LogInformation("Pruning expired logins");
                var expiredLogins = db.Logins.Where(x => x.ExpirationDate <= now);
                db.Logins.RemoveRange(expiredLogins);
                var pruned = await db.SaveChangesAsync(stoppingToken);
                _logger.LogInformation($"Pruned {pruned} expired logins");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to prune expired logins");
            }
        }

        private async Task PruneInactiveDeviceAuthorizations(TurnContext db, DateTimeOffset now, CancellationToken stoppingToken)
        {
            try
            {
                stoppingToken.ThrowIfCancellationRequested();
                _logger.LogInformation("Pruning inactive device authorizations");
                var inactiveDate = now - _appSettings.Value.DeviceInactivityPeriod;
                var inactiveDevices = db.DeviceAuthorizations.Where(x => x.ModifiedDate < inactiveDate);
                db.DeviceAuthorizations.RemoveRange(inactiveDevices);
                var pruned = await db.SaveChangesAsync(stoppingToken);
                _logger.LogInformation($"Pruned {pruned} device authorizations older than {inactiveDate}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to prune inactive device authorizations");
            }
        }
    }
}