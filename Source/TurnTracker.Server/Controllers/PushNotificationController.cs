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
                "This is a test notification to just this device", sub.Endpoint);
            if (result.IsSuccess) return Ok();

            return StatusCode(500);
        }
        
        [HttpPost("test/all")]
        public async Task<IActionResult> TestAll()
        {
            var result = await _pushNotificationService.SendToAllDevicesAsync(User.GetId(), "Test Notification",
                "This is a test notification to all your devices");
            if (result.IsSuccess) return Ok();

            return StatusCode(500);
        }

        #endregion
    }
}