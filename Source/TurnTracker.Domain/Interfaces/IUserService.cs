using CSharpFunctionalExtensions;
using TurnTracker.Data.Entities;

namespace TurnTracker.Domain.Interfaces
{
    public interface IUserService
    {
        Result<(User user, string accessToken, string refreshToken)> AuthenticateUser(string username, string password);
        Result ChangePassword(int userId, string oldPassword, string newPassword);
        Result<string> RefreshUser(int userId, string refreshKey);
        Result<User> GetUser(int userId);
        Result LogoutUser(int userId);
        Result EnsureSeedUsers();
        Result<User> SetDisplayName(int userId, string displayName);
    }
}