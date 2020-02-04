using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
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

        #endregion Fields

        #region Ctor

        public PushNotificationService(PushServiceClient client, IPushSubscriptionService pushService, IOptions<AppSettings> appSettings)
        {
            _pushService = pushService;
            _client = client;
            var config = appSettings.Value.PushNotifications;
            _client.DefaultAuthentication = new VapidAuthentication(config.PublicKey, config.PrivateKey)
            {
                Subject = config.ServerUrl
            };
        }

        #endregion Ctor

        #region Public

        public async Task<Result> SendToOneDeviceAsync(int userId, string title, string message, string endpoint)
        {
            var sub = _pushService.Get(userId, endpoint);
            if (sub is null)
            {
                return Result.Failure("Couldn't find device subscription");
            }

            var pushMessage = BuildMessage(title, message);

            await _client.RequestPushMessageDeliveryAsync(sub, pushMessage);

            return Result.Success();
        }

        public async Task<Result> SendToAllDevicesAsync(int userId, string title, string message)
        {
            var sent = false;
            var pushMessage = BuildMessage(title, message);

            foreach (var sub in _pushService.Get(userId))
            {
                sent = true;
                await _client.RequestPushMessageDeliveryAsync(sub, pushMessage);
            }

            return Result.SuccessIf(sent, "No subscriptions found");
        }

        #endregion Public

        #region Private

        private PushMessage BuildMessage(string title, string message, params AngularPushNotification.NotificationAction[] actions)
        {
            return new AngularPushNotification
            {
                Title = title,
                Body = message,
                Icon = "assets/icons/icon-96x96.png",
                Actions = actions
            }.ToPushMessage();
        }

        #endregion Private
    }
}