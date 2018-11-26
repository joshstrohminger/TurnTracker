using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
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
                Email = "a@b.c", DisplayName = "Admin User",
                Salt = new byte[]{0x03, 0x04},
                Hash = HashPassword(new byte[]{0x03, 0x04}, "letmein")
            }
        };

        public bool Logout(string username)
        {
            var user = _users.SingleOrDefault(x => x.Name == username);
            if (user is null) return false;
            user.AccessToken = null;
            return true;
        }

        public User Authenticate(string username, string password)
        {
            var user = _users.SingleOrDefault(x => x.Name == username && x.Hash.SequenceEqual(HashPassword(x.Salt, password)));

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(ClaimTypes.GivenName, user.DisplayName)
                }),
                Expires = DateTime.UtcNow.Add(_appSettings.LoginExpiration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.AccessToken = tokenHandler.WriteToken(token);

            return user;
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
