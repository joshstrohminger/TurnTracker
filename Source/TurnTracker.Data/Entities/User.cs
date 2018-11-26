using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnTracker.Data.Entities
{
    public sealed class User : Entity
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string Email { get; set; }
        public bool EmailVerified { get; set; }

        [Column(TypeName = "tinyint")]
        public Role Role { get; set; } 

        [Required]
        public byte[] Hash { get; set; }

        [Required]
        public byte[] Salt { get; set; }

        public byte[] ResetToken { get; set; }

        public string AccessToken { get; set; }
    }
}
