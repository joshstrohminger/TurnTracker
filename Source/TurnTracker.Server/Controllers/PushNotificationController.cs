using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TurnTracker.Domain.Configuration;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/notification/push")]
    public class PushNotificationController : Controller
    {
        #region Fields

        private readonly IOptions<AppSettings> _options;

        #endregion

        #region ctor

        public PushNotificationController(IOptions<AppSettings> options)
        {
            _options = options;
        }

        #endregion

        #region Endpoints

        [HttpGet("[action]")]
        public IActionResult PublicKey()
        {
            return Ok(_options.Value.PushNotifications.PublicKey);
        }

        [HttpDelete]
        public IActionResult Unsubscribe(object sub)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public IActionResult Subscribe(object sub)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}