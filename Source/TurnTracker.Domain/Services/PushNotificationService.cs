using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TurnTracker.Domain.Configuration;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Domain.Models;

namespace TurnTracker.Domain.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        #region Fields

        private readonly PushServiceClient _client;
        private readonly IPushSubscriptionService _pushService;
        private readonly string _serverUrl;
        private readonly ILogger<PushNotificationService> _logger;

        #endregion Fields

        #region Ctor

        public PushNotificationService(PushServiceClient client, IPushSubscriptionService pushService, IOptions<AppSettings> appSettings, ILogger<PushNotificationService> logger)
        {
            _pushService = pushService;
            _logger = logger;
            _client = client;
            var config = appSettings.Value.PushNotifications;
            _client.DefaultAuthentication = new VapidAuthentication(config.PublicKey, config.PrivateKey)
            {
                Subject = config.ServerUrl
            };
            _serverUrl = config.ServerUrl;
        }

        #endregion Ctor

        #region Public

        public async Task<Result> SendToOneDeviceAsync(int userId, string title, string message, string endpoint, string groupKey)
        {
            var sub = _pushService.Get(userId, endpoint);
            if (sub is null)
            {
                return Result.Failure("Couldn't find device subscription");
            }

            var notification = BuildMessage(title, message, _serverUrl, groupKey);

            await _client.RequestPushMessageDeliveryAsync(sub, notification.ToPushMessage());

            return Result.Success();
        }

        public async Task<Result> SendCloseToOneDeviceAsync(int userId, string endpoint, string groupKey)
        {
            var sub = _pushService.Get(userId, endpoint);
            if (sub is null)
            {
                return Result.Failure("Couldn't find device subscription");
            }

            var notification = BuildCloseMessage(groupKey);

            await _client.RequestPushMessageDeliveryAsync(sub, notification.ToPushMessage());

            return Result.Success();
        }

        public Result SendToAllDevices(int userId, string title, string message, string url, string groupKey, params PushAction[] actions)
        {
            var sent = false;
            var notification = BuildMessage(title, message, url, groupKey);
            foreach (var action in actions)
            {
                action.ApplyToNotification(notification);
            }

            foreach (var sub in _pushService.Get(userId))
            {
                _logger.LogInformation($"sending push message to {sub.Endpoint}");

                _client.RequestPushMessageDeliveryAsync(sub, notification.ToPushMessage());
                sent = true;
            }

            return Result.SuccessIf(sent, "No subscriptions found");
        }

        public Result SendCloseToAllDevices(int userId, string groupKey)
        {
            var sent = false;
            var notification = BuildCloseMessage(groupKey);

            foreach (var sub in _pushService.Get(userId))
            {
                _logger.LogInformation($"sending close push message to {sub.Endpoint}");

                _client.RequestPushMessageDeliveryAsync(sub, notification.ToPushMessage());
                sent = true;
            }

            return Result.SuccessIf(sent, "No subscriptions found");
        }

        #endregion Public

        #region Private

        private static AngularPushNotification BuildMessage(string title, string message, string url, string groupKey, params AngularPushNotification.NotificationAction[] actions)
        {
            return new AngularPushNotification
            {
                Title = title,
                Body = message,
                Icon = "assets/icons/icon-128x128.png",
                Badge = "assets/icons/icon-72x72.png",
                Renotify = true,
                RequireInteraction = true,
                Tag = groupKey,
                Actions = actions.ToList(),
                Data = new Dictionary<string, object>
                {
                    ["url"] = url
                }
            };
        }

        private static AngularPushNotification BuildCloseMessage(string groupKey)
        {
            return new AngularPushNotification
            {
                Title = "Closing",
                Tag = groupKey,
                Data = new Dictionary<string, object>
                {
                    ["close"] = true
                }
            };
        }

        #endregion Private
    }
}