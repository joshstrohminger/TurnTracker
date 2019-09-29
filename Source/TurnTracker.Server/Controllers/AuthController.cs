using System;
using System.Security.Claims;
using CSharpFunctionalExtensions;
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
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
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
        public IActionResult Logout()
        {
            var myId = User.GetId();
            if (_userService.LogoutUser(myId).IsSuccess)
            {
                _logger.LogWarning($"Failed to logout username: {User.Identity.Name}, id: {myId}");
            }

            return Ok();
        }

        [Authorize(Policy = nameof(PolicyType.Refresh))]
        [HttpPost("[action]")]
        public IActionResult Refresh()
        {
            var (isSuccess, _, accessToken) = _userService.RefreshUser(User.GetId(), User.FindFirstValue(nameof(ClaimType.RefreshKey)));
            if (isSuccess)
            {
                return Ok(accessToken);
            }

            return BadRequest();
        }

        [HttpGet("[action]")]
        public IActionResult Profile()
        {
            var (isSuccess, _, user) = _userService.GetUser(User.GetId());
            if (isSuccess)
            {
                var profile = new UserProfile(user);
                return Ok(profile);
            }

            return BadRequest();
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult AcceptInvite(string token)
        {
            throw new NotImplementedException();
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public string AnonymousData()
        {
            throw new NotImplementedException();
        }

        [HttpGet("[action]")]
        public string UserData()
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
