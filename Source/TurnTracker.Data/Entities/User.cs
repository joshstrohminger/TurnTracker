﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnTracker.Data.Entities
{
    public sealed class User : Entity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }

        [Required]
        public string DisplayName { get; set; }
        [Required]
        public bool IsDisabled { get; set; }
        public Role Role { get; set; }

        public string Email { get; set; }
        public byte[] EmailVerificationHash { get; set; }
        public byte[] EmailVerificationSalt { get; set; }
        public DateTimeOffset? EmailVerificationCreated { get; set; }
        public string EmailBeingVerified { get; set; }

        public string MobileNumber { get; set; }
        public byte[] MobileNumberVerificationHash { get; set; }
        public byte[] MobileNumberVerificationSalt { get; set; }
        public DateTimeOffset? MobileNumberVerificationCreated { get; set; }
        public string MobileNumberBeingVerified { get; set; }
        
        [Required]
        public byte[] PasswordHash { get; set; }
        [Required]
        public byte[] PasswordSalt { get; set; }

        public bool ShowDisabledActivities { get; set; }
        public bool EnablePushNotifications { get; set; }
        public byte SnoozeHours { get; set; }

        #region Navigation

        public List<Participant> Participants { get; set; }

        public List<Turn> TurnsTaken { get; set; }

        public List<Turn> TurnsCreated { get; set; }

        public List<Login> Logins { get; set; }

        public List<DeviceAuthorization> DeviceAuthorizations { get; set; }

        #endregion
    }
}
