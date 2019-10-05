using System.ComponentModel.DataAnnotations;
using TurnTracker.Server.Validators;

namespace TurnTracker.Server.Models
{
    public class PasswordChange
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [MinTrimmedLength(12)]
        public string NewPassword { get; set; }
    }
}