using TurnTracker.Data;
using TurnTracker.Data.Entities;

namespace TurnTracker.Server.Models
{
    public class AuthenticatedUser
    {
        public int Id { get; }
        public string Username { get; }
        public string DisplayName { get; }
        public string RefreshToken { get; }
        public string AccessToken { get; }
        public Role Role { get; }

        public AuthenticatedUser(User user, string accessToken, string refreshToken)
        {
            Id = user.Id;
            Username = user.Username;
            DisplayName = user.DisplayName;
            RefreshToken = refreshToken;
            AccessToken = accessToken;
            Role = user.Role;
        }
    }
}
