using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TurnTracker.Data;
using TurnTracker.Domain.Authorization;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Domain.Models;
using TurnTracker.Server.Models;
using TurnTracker.Server.Utilities;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IWebAuthnService _webAuthnService;
        private readonly ILogger<AuthController> _logger;
        private readonly IResourceAuthorizationService _resourceAuthorizationService;

        public AuthController(IUserService userService, ILogger<AuthController> logger, IWebAuthnService webAuthnService, IResourceAuthorizationService resourceAuthorizationService)
        {
            _userService = userService;
            _logger = logger;
            _webAuthnService = webAuthnService;
            _resourceAuthorizationService = resourceAuthorizationService;
        }

        [HttpPost("[action]")]
        public IActionResult StartDeviceRegistration()
        {
            if (!_resourceAuthorizationService.CanRegisterDevice(User.GetLoginId()))
            {
                return Forbid();
            }

            var (_, isFailure, options, error) = _webAuthnService.MakeCredentialOptions(User.GetId(), User.GetUsername(), User.GetDisplayName(), User.GetLoginId());

            if (isFailure) return BadRequest(error);

            return Json(options);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CompleteDeviceRegistration([FromBody] AuthenticatorAttestationNamedRawResponse response)
        {
            if (response?.RawResponse == null)
            {
                return BadRequest($"Missing {nameof(response.RawResponse)}");
            }
            
            if (string.IsNullOrWhiteSpace(response.DeviceName))
            {
                return BadRequest($"Missing {nameof(response.DeviceName)}");
            }

            var (_, isFailure, _, error) = await _webAuthnService.MakeCredentialAsync(response.RawResponse, User.GetId(), User.GetLoginId(), response.DeviceName);

            if (isFailure) return BadRequest(error);

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult StartDeviceAssertion([FromBody] string username)
        {
            var (_, isFailure, options, error) = _webAuthnService.MakeAssertionOptions(username);

            if (isFailure) return BadRequest(error);

            return Json(options);
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> CompleteDeviceAssertion([FromBody] AnonymousAuthenticatorAssertionRawResponse response)
        {
            var (_, isFailure, (user, accessToken, refreshToken), (unauthorized, errorMessage)) =
                await _webAuthnService.MakeAssertionAsync(response, Request.GetDeviceName());

            if (!isFailure)
            {
                return Ok(new AuthenticatedUser(user, accessToken, refreshToken));
            }

            if (unauthorized)
            {
                return Unauthorized();
            }

            return BadRequest(errorMessage);
        }

        [HttpDelete("[action]/{deviceAuthorizationId}")]
        public async Task<IActionResult> Device(int deviceAuthorizationId)
        {
            if (!_resourceAuthorizationService.CanDeleteDevice(deviceAuthorizationId, User.GetId(), User.GetLoginId()))
            {
                return Forbid();
            }

            var result = await _userService.DeleteDevice(deviceAuthorizationId);
            if (result.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult Login([FromBody] Credentials credentials)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (_, isFailure, (user, accessToken, refreshToken)) = _userService.AuthenticateUser(credentials.Username, credentials.Password, Request.GetDeviceName());
            if (isFailure) return Unauthorized();
            return Ok(new AuthenticatedUser(user, accessToken, refreshToken));
        }

        [HttpPost("[action]")]
        public IActionResult ChangePassword([FromBody] PasswordChange change)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var myId = User.GetId();
            var result = _userService.ChangePassword(myId, change.OldPassword, change.NewPassword);
            if (result.IsFailure) return Unauthorized();
            return Ok();
        }

        [HttpPost("[action]")]
        public IActionResult Logout()
        {
            var loginId = User.GetLoginId();
            if (_userService.Logout(loginId).IsFailure)
            {
                _logger.LogWarning($"Failed to logout username: {User.Identity?.Name}, id: {User.GetId()}, login: {loginId}");
            }

            return Ok();
        }

        [Authorize(Policy = nameof(PolicyType.CanRefreshSession))]
        [HttpPost("[action]")]
        public IActionResult Refresh()
        {
            var (isSuccess, _, accessToken) = _userService.RefreshUser(User.GetLoginId(), User.GetRefreshKey());
            if (isSuccess)
            {
                return Ok(accessToken);
            }

            return BadRequest();
        }
        
        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPost("[action]/{userId}")]
        public IActionResult Reset(int userId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (User.GetId() == userId)
            {
                _logger.LogWarning($"User {userId} tried to reset their own password");
                return BadRequest();
            }

            var result = _userService.ResetPassword(userId);
            if (result.IsSuccess)
            {
                return Ok();
            }

            _logger.LogError($"Failed to reset user id {userId}: {result.Error}");
            return result.Error == ResetPasswordFailure.InvalidUser ? BadRequest() : StatusCode(500);
        }
    }
}
