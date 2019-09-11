using CSharpFunctionalExtensions;
using TurnTracker.Data.Entities;

namespace TurnTracker.Domain.Interfaces
{
    public interface IUserService
    {
        Result<(User user, string refreshToken)> AuthenticateUser(string username, string password);
        Result<string> RefreshUser(string username, string refreshKey);
        Result LogoutUser(string username);
    }
}