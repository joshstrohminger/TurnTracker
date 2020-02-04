namespace TurnTracker.Domain.Configuration
{
    public class PushNotificationSettings
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string ServerUrl { get; set; }
    }
}