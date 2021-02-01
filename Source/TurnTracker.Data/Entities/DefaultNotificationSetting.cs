using System.ComponentModel.DataAnnotations.Schema;

namespace TurnTracker.Data.Entities
{
    public class DefaultNotificationSetting : Entity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public NotificationType Type { get; set; }
        public bool Sms { get; set; }
        public bool Email { get; set; }
        public bool Push { get; set; }

        public int ActivityId { get; set; }
        public Activity Activity { get; set; }
    }
}