using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CSharpFunctionalExtensions;
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
        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        private readonly List<User> _users = new List<User>
        {
            new User { Id = 1, Role = Role.User, Name = "test",
                CreatedDate = DateTimeOffset.Now, ModifiedDate = DateTimeOffset.Now,
                Email = "a@b.c", DisplayName = "Test User",
                Salt = new byte[]{0x01, 0x02},
                Hash = HashPassword(new byte[]{0x01, 0x02}, "letmein")
            },
            new User { Id = 2, Role = Role.Admin, Name = "admin",
                CreatedDate = DateTimeOffset.Now, ModifiedDate = DateTimeOffset.Now,
                Email = "b@b.c", DisplayName = "Admin User",
                Salt = new byte[]{0x03, 0x04},
                Hash = HashPassword(new byte[]{0x03, 0x04}, "letmein")
            }
        };

        public Result LogoutUser(string username)
        {
            var user = _users.SingleOrDefault(x => x.Name == username);
            if (user is null) return Result.Fail("Invalid user");
            user.RefreshKey = null;
            return Result.Ok();
        }

        public Result<(User user, string refreshToken)> AuthenticateUser(string username, string password)
        {
            var user = _users.SingleOrDefault(x => x.Name == username && x.Hash.SequenceEqual(HashPassword(x.Salt, password)));

            // return null if user not found
            if (user == null)
                return Result.Fail<(User, string)>("Invalid credentials");

            // authentication successful so generate jwt refresh token
            var (refreshToken, refreshKey) = GenerateRefreshToken(user);
            user.RefreshKey = refreshKey;

            return Result.Ok((user, refreshToken));
        }

        public Result<string> RefreshUser(string username, string refreshKey)
        {
            var user = _users.SingleOrDefault(x => x.Name == username);

            if (user?.RefreshKey != refreshKey)
                return Result.Fail<string>("Invalid refresh key");

            var accessKey = GenerateAccessToken(user);
            return Result.Ok(accessKey);
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
            var bytes = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(bytes);
            }

            return Convert.ToBase64String(bytes);
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
