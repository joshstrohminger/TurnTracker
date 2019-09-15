using TurnTracker.Data;
using TurnTracker.Data.Entities;

namespace TurnTracker.Server.Models
{
    public class UserProfile
    {
        public int Id { get; }
        public string Username { get; }
        public string DisplayName { get; }
        public Role Role { get; }
        public string MobileNumber { get; }
        public bool MobileNumberVerified { get; }
        public string Email { get; }
        public bool EmailVerified { get; }
        public bool MultiFactorEnabled { get; }

        public UserProfile(User user)
        {
            Id = user.Id;
            Username = user.Name;
            DisplayName = user.DisplayName;
            Role = user.Role;
            MobileNumber = user.MobileNumber;
            MobileNumberVerified = user.MobileNumberVerified;
            Email = user.Email;
            EmailVerified = user.EmailVerified;
            MultiFactorEnabled = user.MultiFactorEnabled;
        }
    }
}