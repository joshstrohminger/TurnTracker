using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TurnTracker.Common;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Configuration;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Domain.Models;

namespace TurnTracker.Domain.HostedServices
{
    public class ActivityStatusChecker : ScopedBackgroundService
    {
        private readonly ILogger<ActivityStatusChecker> _logger;
        private readonly IOptions<AppSettings> _appSettings;

        public ActivityStatusChecker(ILogger<ActivityStatusChecker> logger, IServiceProvider serviceProvider,
            IOptions<AppSettings> appSettings) : base(serviceProvider)
        {
            _logger = logger;
            _appSettings = appSettings;
        }

        protected override async Task<TimeSpan> ExecuteScopedAsync(IServiceProvider services,
            CancellationToken stoppingToken)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var now = DateTimeOffset.Now;
                var db = services.GetRequiredService<TurnContext>();
                var expiredActivities = await db.Activities
                    .Where(activity => !activity.IsDisabled && activity.Due.HasValue && activity.Due <= now)
                    .Include(activity => activity.CurrentTurnUser)
                    .Include(activity => activity.Participants)
                    .ThenInclude(participant => participant.NotificationSettings)
                    .Include(activity => activity.Participants)
                    .ThenInclude(participant => participant.User)
                    .Where(activity => activity.Participants
                        .Any(participant => participant.NotificationSettings
                            .Any(x => x.Push && x.NextCheck <= now &&
                                      (x.Type == NotificationType.OverdueAnybody ||
                                       (x.Type == NotificationType.OverdueMine &&
                                       participant.UserId == activity.CurrentTurnUserId)))))
                    .ToListAsync(stoppingToken);

                _logger.LogInformation($"Found {expiredActivities.Count} expired activities with push notification participants");

                if (expiredActivities.Any())
                {
                    var pushNotificationService = services.GetRequiredService<IPushNotificationService>();
                    var serverUrl = services.GetRequiredService<IOptions<AppSettings>>().Value.PushNotifications
                        .ServerUrl;
                    var userService = services.GetRequiredService<IUserService>();

                    foreach (var activity in expiredActivities)
                    {
                        stoppingToken.ThrowIfCancellationRequested();
                        // ReSharper disable once PossibleInvalidOperationException
                        var overdueTime = (now - activity.Due.Value).ToDisplayString();
                        var notMyTurnBuilder = new StringBuilder();
                        if (activity.CurrentTurnUser != null)
                        {
                            notMyTurnBuilder.AppendFormattable($"It's {activity.CurrentTurnUser.DisplayName}'s turn. ");
                        }
                        notMyTurnBuilder.AppendFormattable($"Overdue by {overdueTime}.");
                        var notMyTurnMessage = notMyTurnBuilder.ToString();
                        var viewUrl = $"{serverUrl}/activity/{activity.Id}";
                        foreach (var participant in activity.Participants)
                        {
                            // ReSharper disable once PossibleInvalidOperationException
                            var myTurn = activity.CurrentTurnUserId == participant.UserId;

                            var pushNotificationSetting = participant.NotificationSettings.FirstOrDefault(x =>
                                x.Push && x.NextCheck <= now && x.Type == NotificationType.OverdueAnybody ||
                                x.Type == NotificationType.OverdueMine && myTurn);

                            if (pushNotificationSetting is null) continue;

                            const string dismissAction = "dismiss";
                            const string snoozeAction = "snooze";
                            var message = myTurn ? $"It's your turn. Overdue by {overdueTime}." : notMyTurnMessage;
                            var dismissToken = userService.GenerateNotificationActionToken(participant, dismissAction,
                                _appSettings.Value.PushNotifications.DefaultExpiration);
                            var snoozeToken = userService.GenerateNotificationActionToken(participant, snoozeAction,
                                _appSettings.Value.PushNotifications.DefaultExpiration);
                            var actionUrl = $"{serverUrl}/api/notification/push/act";
                            if (pushNotificationService.SendToAllDevices(participant.UserId, activity.Name, message,
                                    viewUrl, activity.Id.ToString(),
                                    new PushAction(dismissAction, "Dismiss", actionUrl, dismissToken),
                                    new PushAction(snoozeAction, $"{participant.User.SnoozeHours} Hour{(participant.User.SnoozeHours == 1 ? "" : "s")}", actionUrl, snoozeToken))
                                .IsSuccess)
                            {
                                // keep track of the notification that we sent
                                pushNotificationSetting.NextCheck = now.Add(_appSettings.Value.PushNotifications.DefaultSnooze);
                            }
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }

                _logger.LogInformation($"Done after {stopwatch.Elapsed}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to check activity status");
            }

            return _appSettings.Value.ActivityStatusCheckPeriod;
        }
    }
}