﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
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
        private const int SaltSize = 128 / 8;
        private const int HashSize = 256 / 8;
        private const int HashIterations = 10000;
        private const KeyDerivationPrf HashAlgorithm = KeyDerivationPrf.HMACSHA256;

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
                    var josh = new User
                    {
                        DisplayName = "Joshua",
                        Username = "josh",
                        Email = "josh@mail.com",
                        Role = Role.Admin,
                        MobileNumber = "+1 (888) 123-1337"
                    };
                    AssignNewPassword(josh, "password");

                    var kelly = new User
                    {
                        DisplayName = "Kelly",
                        Username = "kelly",
                        Role = Role.User,
                        MobileNumber = "+1 (666) 123-8008"
                    };
                    AssignNewPassword(kelly, "password");

                    _db.Users.Add(josh);
                    _db.Users.Add(kelly);

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
            var user = _db.Users.SingleOrDefault(x => x.Username == username);
            if (user == null)
                return Result.Fail<(User, string, string)>("Invalid username");

            if(VerifyPassword(user, password).IsFailure)
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

        public Result ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = _db.Users.SingleOrDefault(x => x.Id == userId);
            if (user is null) return Result.Fail("Invalid user");

            if(VerifyPassword(user, oldPassword).IsFailure)
            {
                return Result.Fail("Invalid password");
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
                return Result.Fail<string>("Invalid refresh key or user");

            var accessKey = GenerateAccessToken(user);
            return Result.Ok(accessKey);
        }

        public Result<User> SetDisplayName(int userId, string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return Result.Fail<User>("DisplayName can't be null or empty");
            }

            var user = _db.Users.SingleOrDefault(x => x.Id == userId);
            if (user is null)
            {
                return Result.Fail<User>("Invalid user");
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
                return Result.Fail("Invalid user");
            }

            user.ShowDisabledActivities = show;
            _db.SaveChanges();

            return Result.Ok();
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
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(bytes);
            }

            return bytes;
        }


        private void AssignNewPassword(User user, string password)
        {
            var salt = GetRandomBytes(SaltSize);
            var hash = KeyDerivation.Pbkdf2(password, salt, HashAlgorithm, HashIterations, HashSize);
            user.PasswordSalt = salt;
            user.PasswordHash = hash;
        }

        private Result VerifyPassword(User user, string password)
        {
            if (string.IsNullOrEmpty(password)) return Result.Fail("Invalid password");
            var salt = user.PasswordSalt;
            if (salt is null || salt.Length != SaltSize) return Result.Fail("Invalid salt");
            if (user.PasswordHash is null || user.PasswordHash.Length != HashSize) return Result.Fail("Invalid hash");
            var hash = KeyDerivation.Pbkdf2(password, salt, HashAlgorithm, HashIterations, HashSize);
            return user.PasswordHash.SequenceEqual(hash) ? Result.Ok() : Result.Fail("No match");
        }
    }
}
