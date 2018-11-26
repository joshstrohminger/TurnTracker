using TurnTracker.Data;
using TurnTracker.Data.Entities;

namespace TurnTracker.Server.Models
{
    public class AuthenticatedUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string AccessToken { get; set; }
        public Role Role { get; set; }

        public AuthenticatedUser()
        {
        }

        public AuthenticatedUser(User user)
        {
            Id = user.Id;
            Name = user.Name;
            DisplayName = user.DisplayName;
            AccessToken = user.AccessToken;
            Role = user.Role;
        }
    }
}
