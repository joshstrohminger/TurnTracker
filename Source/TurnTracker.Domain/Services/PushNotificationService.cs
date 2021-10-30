using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public enum PushError
    {
        Unknown,
        NeedsToBeRemoved
    }

    public class PushFailure
    {
        public int UserId { get; }
        public PushSubscription Sub { get; set; }
        public PushError Error { get; }

        public PushFailure(int userId, PushSubscription sub, PushError error)
        {
            UserId = userId;
            Sub = sub ?? throw new ArgumentNullException(nameof(sub));
            Error = error;
        }
    }

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

        public Task<PushFailure[]> SendToAllDevicesAsync(int userId, string title, string message, string url, string groupKey, params PushAction[] actions)
        {
            var notification = BuildMessage(title, message, url, groupKey);
            foreach (var action in actions)
            {
                action.ApplyToNotification(notification);
            }
            var pushMessage = notification.ToPushMessage();
            return SendPushMessageToAllDevicesWithCleanupAsync(userId, pushMessage);
        }

        public Task<PushFailure[]> SendCloseToAllDevicesAsync(int userId, string groupKey)
        {
            var pushMessage = BuildCloseMessage(groupKey).ToPushMessage();
            return SendPushMessageToAllDevicesWithCleanupAsync(userId, pushMessage);
        }

        public async Task CleanupFailuresAsync(IEnumerable<PushFailure> failures)
        {
            foreach (var failure in failures.Where(x => x?.Error == PushError.NeedsToBeRemoved))
            {
                _logger.LogInformation($"Removing sub for user ID {failure.UserId}");
                await _pushService.RemoveSubscriptionAsync(failure.UserId, failure.Sub, false);
            }
        }

        #endregion Public

        #region Private

        private Task<PushFailure[]> SendPushMessageToAllDevicesWithCleanupAsync(int userId, PushMessage pushMessage)
        {
            return Task.WhenAll(_pushService.Get(userId)
                .Select(sub => RequestDeliveryAsync(userId, sub, pushMessage)));
        }

        private async Task<PushFailure> RequestDeliveryAsync(int userId, PushSubscription sub, PushMessage message)
        {
            _logger.LogInformation($"sending close push message to {sub.Endpoint}");

            try
            {
                await _client.RequestPushMessageDeliveryAsync(sub, message);
                return null;
            }
            catch (PushServiceClientException e)
            {
                //https://developers.google.com/web/fundamentals/push-notifications/common-issues-and-reporting-bugs#http_status_codes
                if (e.StatusCode == HttpStatusCode.NotFound || e.StatusCode == HttpStatusCode.Gone)
                {
                    _logger.LogWarning(e,
                        $"Failed to send push message, removing subscription for user ID {userId} to endpoint {sub.Endpoint}");
                    return new PushFailure(userId, sub, PushError.NeedsToBeRemoved);
                }

                _logger.LogError(e,
                    $"Failed to send push message for user ID {userId} to endpoint {sub.Endpoint}, headers: {e.Headers}, body: {e.Body}");
                return new PushFailure(userId, sub, PushError.Unknown);

            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    $"Failed to send push message for user ID {userId} to endpoint {sub.Endpoint}");
                return new PushFailure(userId, sub, PushError.Unknown);
            }
        }

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