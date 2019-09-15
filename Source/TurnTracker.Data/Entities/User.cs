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
        public string Name { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public bool IsDisabled { get; set; }

        [Required]
        public string Email { get; set; }
        public bool EmailVerified { get; set; }

        public string MobileNumber { get; set; }
        public bool MobileNumberVerified { get; set; }

        public bool MultiFactorEnabled { get; set; }

        [Column(TypeName = "tinyint")]
        public Role Role { get; set; } 

        [Required]
        public byte[] Hash { get; set; }

        [Required]
        public byte[] Salt { get; set; }

        public string RefreshKey { get; set; }

        public List<Participant> Participants { get; set; }

        public List<Turn> TurnsTaken { get; set; }

        public List<Turn> TurnsCreated { get; set; }
    }
}
