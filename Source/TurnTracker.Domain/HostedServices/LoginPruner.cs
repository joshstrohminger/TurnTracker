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
    public class LoginPruner : ScopedBackgroundService
    {
        private readonly ILogger<LoginPruner> _logger;
        private readonly IOptions<AppSettings> _appSettings;

        public LoginPruner(IServiceProvider serviceProvider, ILogger<LoginPruner> logger, IOptions<AppSettings> appSettings) : base(serviceProvider)
        {
            _logger = logger;
            _appSettings = appSettings;
        }

        protected override async Task<TimeSpan> ExecuteScopedAsync(IServiceProvider services, CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Running");
                var db = services.GetRequiredService<TurnContext>();
                var now = DateTimeOffset.Now;
                var expiredLogins = db.Logins.Where(x => x.ExpirationDate <= now);
                db.Logins.RemoveRange(expiredLogins);
                var pruned = await db.SaveChangesAsync(stoppingToken);
                _logger.LogInformation($"Pruned {pruned} expired logins");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to prune logins");
            }

            return _appSettings.Value.LoginPrunePeriod;
        }
    }
}