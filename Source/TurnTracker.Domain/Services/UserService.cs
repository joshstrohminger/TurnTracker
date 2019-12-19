using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Authorization;
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
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IOptions<AppSettings> appSettings, TurnContext db, IMapper mapper, ILogger<UserService> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public Result EnsureSeedUsers()
        {
            try
            {
                if (!_db.Users.Any())
                {
                    _logger.LogInformation("Seeding");
                    var josh = new User
                    {
                        DisplayName = "Joshua",
                        Username = "josh",
                        Email = "josh@mail.com",
                        Role = Role.Admin,
                        EnablePushNotifications = true
                    };
                    AssignNewPassword(josh, _appSettings.DefaultPassword);

                    var kelly = new User
                    {
                        DisplayName = "Kelly",
                        Username = "kelly",
                        Role = Role.User
                    };
                    AssignNewPassword(kelly, _appSettings.DefaultPassword);

                    var matt = new User
                    {
                        DisplayName = "Matt",
                        Username = "matt",
                        Role = Role.User
                    };
                    AssignNewPassword(matt, _appSettings.DefaultPassword);

                    _db.Users.Add(josh);
                    _db.Users.Add(kelly);
                    _db.Users.Add(matt);

                    _db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return Result.Failure(e.Message);
            }

            return Result.Ok();
        }

        public Result LogoutUser(int userId)
        {
            var user = _db.Users.SingleOrDefault(x => x.Id == userId);
            if (user is null) return Result.Failure("Invalid user");
            user.RefreshKey = null;
            _db.SaveChanges();
            return Result.Ok();
        }

        public Result<(User user, string accessToken, string refreshToken)> AuthenticateUser(string username, string password)
        {
            var user = _db.Users.SingleOrDefault(x => x.Username == username);
            if (user == null)
                return Result.Failure<(User, string, string)>("Invalid username");

            if(VerifyPassword(user, password).IsFailure)
            {
                return Result.Failure<(User, string, string)>("Invalid password");
            }

            // authentication successful so generate jwt refresh token
            var (refreshToken, refreshKey) = GenerateRefreshToken(user);
            var accessToken = GenerateAccessToken(user);
            user.RefreshKey = refreshKey;
            _db.SaveChanges();

            return Result.Ok((user, accessToken, refreshToken));
        }

        public Result<IEnumerable<UserInfo>> FindUsers(string filter)
        {
            try
            {
                var users = _db.Users
                    .Where(x => x.DisplayName.Contains(filter))
                    .ToList()
                    .Select(x => _mapper.Map<UserInfo>(x));
                return Result.Ok(users);
            }
            catch (Exception e)
            {
                const string message = "Failed to search for users";
                _logger.LogError(e, message);
                return Result.Failure<IEnumerable<UserInfo>>(message);
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

            return Result.Ok();
        }

        public Result<string> RefreshUser(int userId, string refreshKey)
        {
            var user = _db.Users.AsNoTracking().SingleOrDefault(x => x.Id == userId);

            if (user?.RefreshKey != refreshKey)
                return Result.Failure<string>("Invalid refresh key or user");

            var accessKey = GenerateAccessToken(user);
            return Result.Ok(accessKey);
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

            return Result.Ok(user);
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

            return Result.Ok();
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

            return Result.Ok();
        }

        public Result<User> GetUser(int userId)
        {
            var user = _db.Users.AsNoTracking().SingleOrDefault(x => x.Id == userId);
            if (user is null)
            {
                return Result.Failure<User>("invalid user");
            }

            return Result.Ok(user);
        }

        private string GenerateAccessToken(User user)
        {
            return GenerateToken(user, TokenType.Access, _appSettings.AccessTokenExpiration);
        }

        private (string refreshToken, string refreshKey) GenerateRefreshToken(User user)
        {
            var refreshKey = GetRandomKey();
            var claim = new Claim(nameof(ClaimType.RefreshKey), refreshKey);
            return (GenerateToken(user, TokenType.Refresh, _appSettings.RefreshTokenExpiration, claim), refreshKey);
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
            var key = _appSettings.GetSecretBytes();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(expiration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GetRandomKey(ushort length = 100)
        {
            var bytes = GetRandomBytes(length);
            return Convert.ToBase64String(bytes);
        }

        private byte[] GetRandomBytes(ushort length = 50)
        {
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(bytes);
            }

            return bytes;
        }


        private void AssignNewPassword(User user, string password)
        {
            var salt = GetRandomBytes(SaltSize);
            var hash = KeyDerivation.Pbkdf2(password, salt, HashAlgorithm, _appSettings.HashIterations, HashSize);
            user.PasswordSalt = salt;
            user.PasswordHash = hash;
        }

        private Result VerifyPassword(User user, string password)
        {
            if (string.IsNullOrEmpty(password)) return Result.Failure("Invalid password");
            var salt = user.PasswordSalt;
            if (salt is null || salt.Length != SaltSize) return Result.Failure("Invalid salt");
            if (user.PasswordHash is null || user.PasswordHash.Length != HashSize) return Result.Failure("Invalid hash");
            var hash = KeyDerivation.Pbkdf2(password, salt, HashAlgorithm, _appSettings.HashIterations, HashSize);
            return user.PasswordHash.SequenceEqual(hash) ? Result.Ok() : Result.Failure("No match");
        }
    }
}
