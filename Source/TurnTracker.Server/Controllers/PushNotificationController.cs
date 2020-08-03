using System.Threading.Tasks;
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
        public IActionResult Unsubscribe([FromBody] PushSubscription sub)
        {
            if (_pushSubscriptionService.RemoveSubscription(User.GetId(), sub).IsSuccess) return Ok();

            return StatusCode(500);
        }

        [HttpPost("[action]")]
        public IActionResult Subscribe([FromBody] PushSubscription sub)
        {
            if (_pushSubscriptionService.SaveSubscription(User.GetId(), sub).IsSuccess) return Ok();

            return StatusCode(500);
        }

        [HttpPost("test/one")]
        public async Task<IActionResult> TestOne([FromBody] PushSubscription sub)
        {
            var result = await _pushNotificationService.SendToOneDeviceAsync(User.GetId(), "Test Notification",
                "This is a test notification to just this device", sub.Endpoint, "test one");
            if (result.IsSuccess) return Ok();

            return StatusCode(500);
        }

        [HttpPost("test/closeone")]
        public async Task<IActionResult> TestOneClose([FromBody] PushSubscription sub)
        {
            var result = await _pushNotificationService.SendCloseToOneDeviceAsync(User.GetId(), sub.Endpoint, "test one");
            if (result.IsSuccess) return Ok();

            return StatusCode(500);
        }

        [HttpPost("test/all")]
        public IActionResult TestAll()
        {
            var result = _pushNotificationService.SendToAllDevices(User.GetId(), "Test Notification",
                "This is a test notification to all your devices", _options.Value.PushNotifications.ServerUrl, "test all");
            if (result.IsSuccess) return Ok();

            return StatusCode(500);
        }

        [HttpDelete("test/all")]
        public IActionResult TestAllClose()
        {
            var result = _pushNotificationService.SendCloseToAllDevices(User.GetId(), "test all");
            if (result.IsSuccess) return Ok();

            return StatusCode(500);
        }

        #endregion
    }
}