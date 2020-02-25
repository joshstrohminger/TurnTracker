using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Models;

namespace TurnTracker.Domain.Interfaces
{
    public interface IUserService
    {
        Result<(User user, string accessToken, string refreshToken)> AuthenticateUser(string username, string password);
        Result ChangePassword(int userId, string oldPassword, string newPassword);
        Result<string> RefreshUser(long loginId, string refreshKey);
        Result<User> GetUser(int userId);
        Result Logout(long loginId);
        Result EnsureDefaultUsers();
        Result<User> SetDisplayName(int userId, string displayName);
        Result SetShowDisabledActivities(int userId, bool show);
        Result SetEnablePushNotifications(int userId, bool enable);
        Result<IEnumerable<UserInfo>> FindUsers(string filter);
        string GenerateNotificationActionToken(Participant participant, string action, TimeSpan expiration);
        Result SetSnoozeHours(int userId, byte hours);
        Result RegisterDevice(int userId, string info);
    }
}