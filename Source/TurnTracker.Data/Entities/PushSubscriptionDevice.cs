using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TurnTracker.Data.Entities
{
    public class PushSubscriptionDevice : Entity
    {
        public int UserId { get; set; }

        [Required]
        public string Endpoint { get; set; }

        public Dictionary<string, string> Keys { get; set; }

        #region Navigation Properties

        public User User { get; set; }

        #endregion Navigation Properties
    }
}