using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnTracker.Data.Entities
{
    public class Login : Entity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        public string RefreshKey { get; set; }
        public DateTimeOffset ExpirationDate { get; set; }
        public int UserId { get; set; }
        public int? DeviceAuthorizationId { get; set; }
        public string DeviceName { get; set; }

        #region Navigation Properties

        public User User { get; set; }
        public DeviceAuthorization DeviceAuthorization { get; set; }

        #endregion Navigation Properties
    }
}