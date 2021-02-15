using System;
using System.Text;

namespace TurnTracker.Domain.Configuration
{
    public class AppSettings
    {
        #region Types

        public enum SecretEncodingType
        {
            UTF8,
            Base64
        }

        #endregion Types

        #region Properties

        public string Secret { get; set; }
        public SecretEncodingType SecretEncoding { get; set; }
        public TimeSpan AccessTokenExpiration { get; set; }
        public TimeSpan InviteTokenExpiration { get; set; }
        public TimeSpan RefreshTokenExpiration { get; set; }
        public TimeSpan ActivityStatusCheckPeriod { get; set; }
        public TimeSpan EmailVerificationExpiration { get; set; }
        public TimeSpan MobileNumberVerificationExpiration { get; set; }
        public string DefaultPassword { get; set; }
        public string[] DefaultAdmins { get; set; }
        public string[] DefaultUsers { get; set; }
        public int HashIterations { get; set; }
        public bool Seed { get; set; }
        public TimeSpan JwtClockSkew { get; set; }
        public PushNotificationSettings PushNotifications { get; set; }
        public TimeSpan PrunePeriod { get; set; }
        public TimeSpan ChallengeExpiration { get; set; }
        public TimeSpan DeviceInactivityPeriod { get; set; }
        public bool ValidateActivityModifiedDate { get; set; }

        #endregion Properties

        #region Methods

        public byte[] GetSecretBytes()
        {
            return SecretEncoding switch
            {
                SecretEncodingType.Base64 => Convert.FromBase64String(Secret),
                SecretEncodingType.UTF8 => Encoding.UTF8.GetBytes(Secret),
                _ => throw new ArgumentOutOfRangeException(nameof(SecretEncoding))
            };
        }

        #endregion Methods
    }
}
