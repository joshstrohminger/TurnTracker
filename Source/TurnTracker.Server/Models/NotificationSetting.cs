using System.ComponentModel.DataAnnotations;
using TurnTracker.Data.Entities;
using TurnTracker.Server.Utilities;

namespace TurnTracker.Server.Models
{
    public class NotificationSetting
    {
        [InEnum]
        public NotificationType Type { get; set; }

        public bool Sms { get; set; }
        public bool Email { get; set; }
        public bool Push { get; set; }

        [Range(1, int.MaxValue)]
        public int ParticipantId { get; set; }
    }
}