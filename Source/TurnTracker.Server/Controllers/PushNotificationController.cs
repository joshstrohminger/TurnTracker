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
        private readonly IPushNotificationService _pushNotificationService;

        #endregion

        #region ctor

        public PushNotificationController(IOptions<AppSettings> options, IPushNotificationService pushNotificationService)
        {
            _options = options;
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
            if (_pushNotificationService.RemoveSubscription(User.GetId(), sub).IsSuccess) return Ok();

            return StatusCode(500);
        }

        [HttpPost("[action]")]
        public IActionResult Subscribe([FromBody] PushSubscription sub)
        {
            if (_pushNotificationService.SaveSubscription(User.GetId(), sub).IsSuccess) return Ok();

            return StatusCode(500);
        }

        [HttpPost("[action]")]
        public IActionResult TestOne([FromBody] PushSubscription sub)
        {
            if (_pushNotificationService.SendToOneDevice(User.GetId(), "This is a test notification to just this device", sub.Endpoint).IsSuccess) return Ok();

            return StatusCode(500);
        }
        
        [HttpPost("[action]")]
        public IActionResult TestAll()
        {
            if (_pushNotificationService.SendToAllDevices(User.GetId(), "This is a test notification to all your devices").IsSuccess) return Ok();

            return StatusCode(500);
        }

        #endregion
    }
}