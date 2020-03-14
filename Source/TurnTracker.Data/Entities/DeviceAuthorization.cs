﻿using System.ComponentModel.DataAnnotations;

namespace TurnTracker.Data.Entities
{
    public class DeviceAuthorization : Entity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required]
        public string ClientInfo { get; set; }

        #region Navigation Properties

        public User User { get; set; }

        #endregion Navigation Properties
    }
}