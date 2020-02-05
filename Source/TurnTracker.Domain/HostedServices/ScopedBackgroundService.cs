using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TurnTracker.Domain.HostedServices
{
    public abstract class ScopedBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        protected ScopedBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Called for each individual scoped execution of the service. A new scope will be opened for each call.
        /// </summary>
        /// <param name="scopedServiceProvider">Service provider tied to the scope of this individual execution.</param>
        /// <param name="stoppingToken">Used to stop all operations.</param>
        /// <returns>The time to wait before running again with a new scope.</returns>
        protected abstract Task<TimeSpan> ExecuteScopedAsync(IServiceProvider scopedServiceProvider, CancellationToken stoppingToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TimeSpan delay;
                using (var scope = _serviceProvider.CreateScope())
                {
                    delay = await ExecuteScopedAsync(scope.ServiceProvider, stoppingToken);
                }
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}