using System;

namespace TurnTracker.Domain
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string DatabaseConnection { get; set; }
        public TimeSpan LoginExpiration { get; set; }
    }
}
