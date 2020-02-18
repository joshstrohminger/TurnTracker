using System;

namespace TurnTracker.Domain.Configuration
{
    public class PushNotificationSettings
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string ServerUrl { get; set; }
        public TimeSpan DefaultExpiration { get; set; }
        public TimeSpan DefaultSnooze { get; set; }
        public TimeSpan DefaultDismissTime { get; set; }
    }
}