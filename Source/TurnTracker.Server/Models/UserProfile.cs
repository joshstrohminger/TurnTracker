using System;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Configuration;

namespace TurnTracker.Server.Models
{
    public class UserProfile
    {
        public enum VerificationStatus
        {
            None,
            Pending,
            Expired,
            Verified
        }

        public int Id { get; }
        public string Username { get; }
        public string DisplayName { get; }
        public Role Role { get; }
        public string MobileNumber { get; }
        public VerificationStatus MobileNumberVerification { get; }
        public string Email { get; }
        public VerificationStatus EmailVerification { get; }

        public UserProfile(User user, AppSettings appSettings)
        {
            Id = user.Id;
            Username = user.Username;
            DisplayName = user.DisplayName;
            Role = user.Role;
            MobileNumber = user.MobileNumber;
            MobileNumberVerification = DetermineVerificationStatus(user.MobileNumber, user.MobileNumberBeingVerified,
                user.MobileNumberVerificationCreated, appSettings.MobileNumberVerificationExpiration);
            Email = user.Email;
            EmailVerification = DetermineVerificationStatus(user.Email, user.EmailBeingVerified, user.EmailVerificationCreated,
                appSettings.EmailVerificationExpiration);
        }

        private VerificationStatus DetermineVerificationStatus(string currentItem, string itemBeingVerified, DateTimeOffset? created, TimeSpan expiration)
        {
            if (currentItem != null) return VerificationStatus.Verified;
            if (itemBeingVerified is null || !created.HasValue) return VerificationStatus.None;
            if (created.Value + expiration > DateTimeOffset.Now) return VerificationStatus.Pending;
            return VerificationStatus.Expired;
        }
    }
}