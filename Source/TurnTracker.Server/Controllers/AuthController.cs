using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Fido2NetLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TurnTracker.Data;
using TurnTracker.Domain.Authorization;
using TurnTracker.Domain.Interfaces;
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

        public AuthController(IUserService userService, ILogger<AuthController> logger, IWebAuthnService webAuthnService)
        {
            _userService = userService;
            _logger = logger;
            _webAuthnService = webAuthnService;
        }

        [HttpPost("[action]")]
        public IActionResult StartDeviceRegistration()
        {
            var (_, isFailure, options, error) = _webAuthnService.MakeCredentialOptions(User.GetId(), User.GetUsername(), User.GetDisplayName(), User.GetLoginId());

            if (isFailure) return BadRequest(error);

            return Json(options);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CompleteDeviceRegistration([FromBody] AuthenticatorAttestationRawResponse response)
        {
            var (_, isFailure, credential, error) = await _webAuthnService.MakeCredentialAsync(response, User.GetId(), User.GetLoginId());

            if (isFailure) return BadRequest(error);

            return Json(credential);
        }

        [HttpPost("[action]")]
        public IActionResult StartDeviceAssertion()
        {
            var (_, isFailure, options, error) = _webAuthnService.MakeAssertionOptions(User.GetId());

            if (isFailure) return BadRequest(error);

            return Json(options);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CompleteDeviceAssertion([FromBody] AuthenticatorAssertionRawResponse response)
        {
            var (_, isFailure, error) = await _webAuthnService.MakeAssertionAsync(User.GetId(), response);

            if (isFailure) return BadRequest(error);

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult Login([FromBody] Credentials credentials)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (_, isFailure, (user, accessToken, refreshToken)) = _userService.AuthenticateUser(credentials.Username, credentials.Password);
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
            if (_userService.Logout(loginId).IsSuccess)
            {
                _logger.LogWarning($"Failed to logout username: {User.Identity.Name}, id: {User.GetId()}, login: {loginId}");
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

        public IActionResult CreateCredentialOptions()
        {
            throw new NotImplementedException();
        }
        
        [Authorize(Roles = nameof(Role.Admin))]
        [HttpGet("[action]")]
        public string AdminData()
        {
            throw new NotImplementedException();
        }
    }
}
