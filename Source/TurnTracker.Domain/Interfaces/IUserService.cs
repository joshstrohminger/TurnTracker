using TurnTracker.Data.Entities;

namespace TurnTracker.Domain.Interfaces
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        bool Logout(string username);
    }
}