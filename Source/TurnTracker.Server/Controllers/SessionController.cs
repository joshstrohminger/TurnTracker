using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Utilities;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SessionController : Controller
    {
        #region Fields

        private readonly IResourceAuthorizationService _resourceAuthorizationService;
        private readonly IUserService _userService;

        #endregion Fields

        #region Ctor

        public SessionController(IResourceAuthorizationService resourceAuthorizationService, IUserService userService)
        {
            _resourceAuthorizationService = resourceAuthorizationService;
            _userService = userService;
        }

        #endregion Ctor

        #region Endpoints

        [HttpGet]
        public IActionResult GetAllSessions()
        {
            return Json(_userService.GetAllSessionsByDevice(User.GetId(), User.GetLoginId()));
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> Web()
        {
            var result = await _userService.DeleteWebLogins(User.GetId(), User.GetLoginId());
            if (result.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok();
        }

        [HttpDelete("{loginId}")]
        public async Task<IActionResult> DeleteSession(long loginId)
        {
            var currentLoginId = User.GetLoginId();
            if (currentLoginId == loginId || !_resourceAuthorizationService.CanDeleteSession(loginId, User.GetId()))
            {
                return Unauthorized();
            }

            var result = await _userService.DeleteLogin(loginId);
            if (result.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok();
        }

        #endregion Endpoints
    }
}