using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnTracker.Data.Entities
{
    public class Participant : Entity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int ActivityId { get; set; }
        public Activity Activity { get; set; }

        public int TurnsNeeded { get; set; }
        public int TurnOrder { get; set; }

        public bool HasDisabledTurns { get; set; }

        public List<NotificationSetting> NotificationSettings { get; set; }
    }
}
