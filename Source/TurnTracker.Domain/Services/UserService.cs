using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Authorization;
using TurnTracker.Domain.Interfaces;

namespace TurnTracker.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly TurnContext _db;
        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings, TurnContext db)
        {
            _db = db;
            _appSettings = appSettings.Value;
        }

        public Result EnsureSeedUsers()
        {
            try
            {
                if (!_db.Users.Any())
                {
                    var salt = GetRandomBytes();
                    var now = DateTimeOffset.Now;
                    _db.Users.Add(new User
                    {
                        DisplayName = "Joshua",
                        Name = "josh",
                        Email = "josh@mail.com",
                        Role = Role.Admin,
                        Salt = salt,
                        Hash = HashPassword(salt, "password"),
                        MobileNumber = "+1 (888) 123-4567",
                        MobileNumberVerified = true
                    });
                    salt = GetRandomBytes();
                    _db.Users.Add(new User
                    {
                        DisplayName = "Kelly",
                        Name = "kelly",
                        Email = "kelly@mail.com",
                        Role = Role.User,
                        Salt = salt,
                        Hash = HashPassword(salt, "password")
                    });
                    _db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }


            return Result.Ok();
        }

        public Result LogoutUser(int userId)
        {
            var user = _db.Users.SingleOrDefault(x => x.Id == userId);
            if (user is null) return Result.Fail("Invalid user");
            user.RefreshKey = null;
            _db.SaveChanges();
            return Result.Ok();
        }

        public Result<(User user, string accessToken, string refreshToken)> AuthenticateUser(string username, string password)
        {
            var user = _db.Users.SingleOrDefault(x => x.Name == username);
            if (user == null)
                return Result.Fail<(User, string, string)>("Invalid username");

            if (!user.Hash.SequenceEqual(HashPassword(user.Salt, password)))
            {
                return Result.Fail<(User, string, string)>("Invalid password");
            }

            // authentication successful so generate jwt refresh token
            var (refreshToken, refreshKey) = GenerateRefreshToken(user);
            var accessToken = GenerateAccessToken(user);
            user.RefreshKey = refreshKey;
            _db.SaveChanges();

            return Result.Ok((user, accessToken, refreshToken));
        }

        public Result<string> RefreshUser(int userId, string refreshKey)
        {
            var user = _db.Users.AsNoTracking().SingleOrDefault(x => x.Id == userId);

            if (user?.RefreshKey != refreshKey)
                return Result.Fail<string>("Invalid refresh key or user");

            var accessKey = GenerateAccessToken(user);
            return Result.Ok(accessKey);
        }

        public Result<User> GetUser(int userId)
        {
            var user = _db.Users.AsNoTracking().SingleOrDefault(x => x.Id == userId);
            if (user is null)
            {
                return Result.Fail<User>("invalid user");
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
                new Claim(ClaimTypes.Name, user.Name),
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
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
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
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(bytes);
            }

            return bytes;
        }

        private static byte[] HashPassword(byte[] salt, string password)
        {
            if(salt is null || salt.Length == 0) throw new ArgumentNullException(nameof(salt));

            using (var hasher = new HMACSHA256(salt))
            {
                return hasher.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
