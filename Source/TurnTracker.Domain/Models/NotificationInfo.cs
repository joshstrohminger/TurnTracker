using TurnTracker.Data.Entities;

namespace TurnTracker.Domain.Models
{
    public class NotificationInfo
    {
        public NotificationType Type { get; set; }
        public bool Sms { get; set; }
        public bool Email { get; set; }
        public bool Push { get; set; }
        public int ParticipantId { get; set; }

        public NotificationInfo(NotificationSetting note)
        {
            Type = note.Type;
            Sms = note.Sms;
            Email = note.Email;
            Push = note.Push;
            ParticipantId = note.ParticipantId;
        }

        public NotificationInfo(DefaultNotificationSetting note)
        {
            Type = note.Type;
            Sms = note.Sms;
            Email = note.Email;
            Push = note.Push;
        }
    }
}