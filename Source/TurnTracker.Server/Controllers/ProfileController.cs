using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TurnTracker.Domain;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Models;
using TurnTracker.Server.Utilities;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IOptions<AppSettings> _appSettings;

        public ProfileController(IUserService userService, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _appSettings = appSettings;
        }

        [HttpGet]
        public IActionResult GetProfile()
        {
            var (isSuccess, _, user) = _userService.GetUser(User.GetId());
            if (isSuccess)
            {
                var profile = new UserProfile(user, _appSettings.Value);
                return Json(profile);
            }

            return BadRequest();
        }

        [HttpPut("displayname")]
        public IActionResult SetDisplayName([FromBody] string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return BadRequest("DisplayName can't be null or whitespace");
            }

            var myId = User.GetId();
            var (isSuccess, _, user) = _userService.SetDisplayName(myId, displayName);
            if (isSuccess)
            {
                var profile = new UserProfile(user, _appSettings.Value);
                return Json(profile);
            }

            return StatusCode(500);
        }
    }
}