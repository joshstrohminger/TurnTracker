using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnTracker.Data.Entities
{
    public enum NotificationType
    {
        TurnTakenAnybody,
        TurnTakenMine,
        OverdueAnybody,
        OverdueMine
    }

    public class NotificationSetting : Entity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public NotificationType Type { get; set; }
        public bool Sms { get; set; }
        public bool Email { get; set; }
        public bool Push { get; set; }

        public DateTimeOffset NextCheck { get; set; }

        public int ParticipantId { get; set; }
        public Participant Participant { get; set; }
    }
}