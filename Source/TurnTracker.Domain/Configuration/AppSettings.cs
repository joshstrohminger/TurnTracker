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
        public TimeSpan EmailVerificationExpiration { get; set; }
        public TimeSpan MobileNumberVerificationExpiration { get; set; }
        public string DefaultPassword { get; set; }
        public int HashIterations { get; set; }
        public bool Seed { get; set; }
        public TimeSpan JwtClockSkew { get; set; }
        public PushNotificationSettings PushNotifications { get; set; }

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
