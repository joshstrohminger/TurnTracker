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
        public Role Role { get; }

        public AuthenticatedUser(User user, string refreshToken)
        {
            Id = user.Id;
            Username = user.Name;
            DisplayName = user.DisplayName;
            RefreshToken = refreshToken;
            Role = user.Role;
        }
    }
}
