using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TurnTracker.Common;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Authorization;
using TurnTracker.Domain.Configuration;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Domain.Models;

namespace TurnTracker.Domain.Services
{
    public class UserService : IUserService
    {
        private const int SaltSize = 128 / 8;
        private const int HashSize = 256 / 8;
        private const KeyDerivationPrf HashAlgorithm = KeyDerivationPrf.HMACSHA256;

        private readonly TurnContext _db;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IOptions<AppSettings> appSettings, TurnContext db, IMapper mapper, ILogger<UserService> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _appSettings = appSettings;
        }

        public Result EnsureDefaultUsers()
        {
            try
            {
                if (!_db.Users.Any())
                {
                    _logger.LogInformation("Seeding");
                    foreach (var user in _appSettings.Value.DefaultAdmins
                        .EmptyIfNull()
                        .Select(username => new User
                        {
                            DisplayName = username,
                            Username = username,
                            Role = Role.Admin
                        })
                        .Concat(_appSettings.Value.DefaultUsers
                            .EmptyIfNull()
                            .Select(username => new User
                        {
                            DisplayName = username,
                            Username = username,
                            Role = Role.User
                        })))
                    {
                        AssignNewPassword(user, _appSettings.Value.DefaultPassword);
                        _db.Users.Add(user);
                    }

                    _db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return Result.Failure(e.Message);
            }

            return Result.Success();
        }

        public Result Logout(long loginId)
        {
            var login = _db.Logins.Find(loginId);
            if (login is null) return Result.Failure("Invalid login");
            _db.Logins.Remove(login);
            _db.SaveChanges();
            return Result.Success();
        }

        public Result<(User user, string accessToken, string refreshToken)> AuthenticateUser(string username, string password, string deviceName)
        {
            var user = _db.Users.SingleOrDefault(x => x.Username == username);
            if (user == null)
                return Result.Failure<(User, string, string)>("Invalid username");

            if(VerifyPassword(user, password).IsFailure)
            {
                return Result.Failure<(User, string, string)>("Invalid password");
            }

            // authentication successful so generate jwt refresh token
            return Result.Success(GenerateAndSaveLogin(user, deviceName));
        }

        /// <summary>
        /// Create a new login and associated tokens and save any outstanding db changes
        /// </summary>
        /// <param name="user">The user associated with the login</param>
        /// <param name="deviceName">Name of the device used to login</param>
        /// <param name="deviceAuthorizationId">ID of the device used to login, or <c>null</c> if no device was used.</param>
        public (User user, string accessToken, string refreshToken) GenerateAndSaveLogin(User user, string deviceName, int? deviceAuthorizationId = null)
        {
            var (refreshToken, loginId) = GenerateRefreshToken(user, deviceName, deviceAuthorizationId);
            var accessToken = GenerateAccessToken(user, loginId);
            _db.SaveChanges();

            return (user, accessToken, refreshToken);
        }

        public Result<IEnumerable<UserInfo>> FindUsers(string filter)
        {
            try
            {
                var users = _db.Users
                    .Where(x => x.DisplayName.Contains(filter))
                    .ToList()
                    .Select(x => _mapper.Map<UserInfo>(x));
                return Result.Success(users);
            }
            catch (Exception e)
            {
                const string message = "Failed to search for users";
                _logger.LogError(e, message);
                return Result.Failure<IEnumerable<UserInfo>>(message);
            }
        }

        public async Task<Result> DeleteDevice(int deviceAuthorizationId)
        {
            try
            {
                var device = await _db.DeviceAuthorizations
                    .Include(x => x.Logins)
                    .SingleOrDefaultAsync(x => x.Id == deviceAuthorizationId);
                if (device != null)
                {
                    _db.RemoveRange(device.Logins);
                    _db.Remove(device);
                    await _db.SaveChangesAsync();
                }
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to delete device {deviceAuthorizationId}", e);
                return Result.Failure("Failed to delete device");
            }
        }

        public async Task<Result> DeleteWebLogins(int userId, long loginId)
        {
            try
            {
                var logins = _db.Logins.Where(x => x.UserId == userId && x.Id != loginId && x.DeviceAuthorizationId == null);
                _db.RemoveRange(logins);
                await _db.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to delete web logins", e);
                return Result.Failure("Failed to delete web session");
            }
        }

        public async Task<Result> DeleteLogin(long loginId)
        {
            try
            {
                var login = await _db.Logins.FindAsync(loginId);
                if (login != null)
                {
                    _db.Remove(login);
                    await _db.SaveChangesAsync();
                }
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to delete login {loginId}", e);
                return Result.Failure("Failed to delete session");
            }
        }

        public Result ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = _db.Users.SingleOrDefault(x => x.Id == userId);
            if (user is null) return Result.Failure("Invalid user");

            if(VerifyPassword(user, oldPassword).IsFailure)
            {
                return Result.Failure("Invalid password");
            }

            // authentication successful set the new password
            AssignNewPassword(user, newPassword);
            _db.SaveChanges();

            return Result.Success();
        }

        public Result<string> RefreshUser(long loginId, string refreshKey)
        {
            var login = _db.Logins.Include(x => x.User).SingleOrDefault(x => x.Id == loginId);

            if (login is null)
            {
                _logger.LogWarning($"Attempted to refresh missing login id {loginId}");
                return Result.Failure<string>("Invalid login id");
            }

            if (login.RefreshKey != refreshKey)
            {
                _logger.LogWarning($"Attempted to refresh login id {loginId} with invalid refresh key");
                return Result.Failure<string>("Invalid refresh key");
            }

            // mark entity as modified so the modified date will get updated
            _db.Entry(login).State = EntityState.Modified;
            _db.SaveChanges();

            var accessKey = GenerateAccessToken(login.User, loginId);
            return Result.Success(accessKey);
        }

        public Result<User> SetDisplayName(int userId, string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return Result.Failure<User>("DisplayName can't be null or empty");
            }

            var user = _db.Users.SingleOrDefault(x => x.Id == userId);
            if (user is null)
            {
                return Result.Failure<User>("Invalid user");
            }

            user.DisplayName = displayName.Trim();
            _db.SaveChanges();

            return Result.Success(user);
        }

        public Result SetShowDisabledActivities(int userId, bool show)
        {
            var user = _db.Users.SingleOrDefault(x => x.Id == userId);
            if (user is null)
            {
                return Result.Failure("Invalid user");
            }

            user.ShowDisabledActivities = show;
            _db.SaveChanges();

            return Result.Success();
        }

        public IEnumerable<Device> GetAllSessionsByDevice(int userId, long loginId)
        {
            var user = _db.Users.AsNoTracking()
                .Include(u => u.Logins)
                .Include(u => u.DeviceAuthorizations)
                .Single(u => u.Id == userId);

            var devices = user.DeviceAuthorizations.ToDictionary(d => d.Id, d => new Device
            {
                Name = d.DeviceName,
                Id = d.Id,
                Created = d.CreatedDate,
                Updated = d.ModifiedDate,
                Sessions = Enumerable.Empty<Session>()
            });

            foreach (var g in user.Logins.GroupBy(l => l.DeviceAuthorizationId))
            {
                var sessions = g.OrderByDescending(l => l.ModifiedDate).Select(login => new Session
                {
                    Id = login.Id,
                    Name = login.DeviceName,
                    Updated = login.ModifiedDate,
                    Created = login.CreatedDate,
                    Current = login.Id == loginId
                }).ToList();

                if (g.Key.HasValue)
                {
                    var device = devices[g.Key.Value];
                    device.Current = sessions.Any(s => s.Id == loginId);
                    device.Sessions = sessions;
                }
                else
                {
                    devices[0] = new Device
                    {
                        Name = "Web",
                        Created = user.CreatedDate,
                        Current = sessions.Any(s => s.Id == loginId),
                        Updated = sessions.Max(s => s.Updated),
                        Sessions = sessions
                    };
                }
            }

            return devices.Values.OrderByDescending(d => d.Updated);
        }

        public Result SetEnablePushNotifications(int userId, bool enable)
        {
            var user = _db.Users.SingleOrDefault(x => x.Id == userId);
            if (user is null)
            {
                return Result.Failure("Invalid user");
            }

            user.EnablePushNotifications = enable;
            _db.SaveChanges();

            return Result.Success();
        }

        public Result SetSnoozeHours(int userId, byte hours)
        {
            if (hours == 0)
            {
                return Result.Failure("Hours can't be zero");
            }

            var user = _db.Users.SingleOrDefault(x => x.Id == userId);
            if (user is null)
            {
                return Result.Failure("Invalid user");
            }

            user.SnoozeHours = hours;
            _db.SaveChanges();

            return Result.Success();
        }

        public Result<User> GetUser(int userId)
        {
            var user = _db.Users.AsNoTracking().SingleOrDefault(x => x.Id == userId);
            if (user is null)
            {
                return Result.Failure<User>("invalid user");
            }

            return Result.Success(user);
        }

        private string GenerateAccessToken(User user, long loginId)
        {
            var loginClaim = new Claim(nameof(ClaimType.LoginId), loginId.ToString());
            return GenerateToken(user, TokenType.Access, _appSettings.Value.AccessTokenExpiration, loginClaim);
        }

        private (string refreshToken, long loginId) GenerateRefreshToken(User user, string deviceName, int? deviceAuthorizationId)
        {
            var refreshKey = GetRandomKey();
            var login = new Login
            {
                UserId = user.Id,
                RefreshKey = refreshKey,
                ExpirationDate = DateTimeOffset.Now + _appSettings.Value.RefreshTokenExpiration,
                DeviceAuthorizationId = deviceAuthorizationId,
                DeviceName = deviceName
            };
            _db.Logins.Add(login);
            _db.SaveChanges();
            var keyClaim = new Claim(nameof(ClaimType.RefreshKey), refreshKey);
            var loginClaim = new Claim(nameof(ClaimType.LoginId), login.Id.ToString());
            return (GenerateToken(user, TokenType.Refresh, _appSettings.Value.RefreshTokenExpiration, keyClaim, loginClaim), login.Id);
        }

        public string GenerateNotificationActionToken(Participant participant, string action, TimeSpan expiration)
        {
            var actionClaim = new Claim(nameof(ClaimType.NotificationAction), action);
            var participantClaim = new Claim(nameof(ClaimType.ParticipantId), participant.Id.ToString());
            return GenerateToken(participant.User, TokenType.Notification, expiration, actionClaim, participantClaim);
        }

        private string GenerateToken(User user, TokenType tokenType, TimeSpan expiration, params Claim[] additionalClaims)
        {
            IEnumerable<Claim> claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.GivenName, user.DisplayName),
                new Claim(nameof(ClaimType.TokenType), tokenType.ToString()),
            };
            if (additionalClaims?.Length > 0)
            {
                claims = claims.Concat(additionalClaims);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _appSettings.Value.GetSecretBytes();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(expiration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string GetRandomKey(ushort length = 100)
        {
            var bytes = GetRandomBytes(length);
            return Convert.ToBase64String(bytes);
        }

        private static byte[] GetRandomBytes(ushort length = 50)
        {
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetNonZeroBytes(bytes);
            return bytes;
        }


        private void AssignNewPassword(User user, string password)
        {
            var salt = GetRandomBytes(SaltSize);
            var hash = KeyDerivation.Pbkdf2(password, salt, HashAlgorithm, _appSettings.Value.HashIterations, HashSize);
            user.PasswordSalt = salt;
            user.PasswordHash = hash;
        }

        private Result VerifyPassword(User user, string password)
        {
            if (string.IsNullOrEmpty(password)) return Result.Failure("Invalid password");
            var salt = user.PasswordSalt;
            if (salt is null || salt.Length != SaltSize) return Result.Failure("Invalid salt");
            if (user.PasswordHash is null || user.PasswordHash.Length != HashSize) return Result.Failure("Invalid hash");
            var hash = KeyDerivation.Pbkdf2(password, salt, HashAlgorithm, _appSettings.Value.HashIterations, HashSize);
            return user.PasswordHash.SequenceEqual(hash) ? Result.Success() : Result.Failure("No match");
        }
    }
}
