using System;
using System.Text;

namespace TurnTracker.Domain
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
        public string DatabaseConnection { get; set; }
        public TimeSpan AccessTokenExpiration { get; set; }
        public TimeSpan InviteTokenExpiration { get; set; }
        public TimeSpan RefreshTokenExpiration { get; set; }
        public TimeSpan EmailVerificationExpiration { get; set; }
        public TimeSpan MobileNumberVerificationExpiration { get; set; }

        #endregion Properties

        #region Methods

        public byte[] GetSecretBytes()
        {
            switch(SecretEncoding)
            {
                case SecretEncodingType.Base64:
                    return Convert.FromBase64String(Secret);
                case SecretEncodingType.UTF8:
                    return Encoding.UTF8.GetBytes(Secret);
                default:
                    throw new ArgumentOutOfRangeException(nameof(SecretEncoding));
            }
        }

        #endregion Methods
    }
}
