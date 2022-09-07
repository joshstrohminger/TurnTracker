using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TurnTracker.Domain.Configuration;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Utilities;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/notification/push")]
    public class PushNotificationController : Controller
    {
        #region Fields

        private readonly IOptions<AppSettings> _options;
        private readonly IPushSubscriptionService _pushSubscriptionService;
        private readonly IPushNotificationService _pushNotificationService;

        #endregion

        #region ctor

        public PushNotificationController(IOptions<AppSettings> options, IPushSubscriptionService pushSubscriptionService, IPushNotificationService pushNotificationService)
        {
            _options = options;
            _pushSubscriptionService = pushSubscriptionService;
            _pushNotificationService = pushNotificationService;
        }

        #endregion

        #region Endpoints

        [AllowAnonymous]
        [HttpGet("[action]")]
        public IActionResult PublicKey()
        {
            return Ok(_options.Value.PushNotifications.PublicKey);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Unsubscribe([FromBody] PushSubscription sub)
        {
            var (isSuccess, _) = await _pushSubscriptionService.RemoveSubscriptionAsync(User.GetId(), sub, true);
            return isSuccess ? Ok() : StatusCode(500);
        }

        [HttpPost("[action]")]
        public IActionResult Subscribe([FromBody] PushSubscription sub)
        {
            var (isSuccess, _, error) = _pushSubscriptionService.SaveSubscription(User.GetId(), sub);
            return isSuccess ? Ok() : StatusCode(500, error);
        }

        [HttpPost("test/one")]
        public async Task<IActionResult> TestOne([FromBody] PushSubscription sub)
        {
            var (isSuccess, _) = await _pushNotificationService.SendToOneDeviceAsync(User.GetId(), "Test Notification",
                "This is a test notification to just this device", sub.Endpoint, "test one");
            return isSuccess ? Ok() : StatusCode(500);
        }

        [HttpPost("test/closeone")]
        public async Task<IActionResult> TestOneClose([FromBody] PushSubscription sub)
        {
            var (isSuccess, _) = await _pushNotificationService.SendCloseToOneDeviceAsync(User.GetId(), sub.Endpoint, "test one");
            return isSuccess ? Ok() : StatusCode(500);
        }

        [HttpPost("test/all")]
        public async Task<IActionResult> TestAll()
        {
            var failure = await _pushNotificationService.SendToAllDevicesAsync("test", User.GetId(), "Test Notification",
                "This is a test notification to all your devices", _options.Value.PushNotifications.ServerUrl, "test all");
            return failure is null ? StatusCode(500) : Ok();
        }

        [HttpDelete("test/all")]
        public async Task<IActionResult> TestAllClose()
        {
            var failure = await _pushNotificationService.SendCloseToAllDevicesAsync("test", User.GetId(), "test all");
            return failure is null ? StatusCode(500) : Ok();
        }

        #endregion
    }
}