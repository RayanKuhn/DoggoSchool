using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilRouge.Models
{
    public class Participate
    {
        [Key]
        public int ParticipateId { get; set; }

        [Required]
        public int SessionId { get; set; }

        [ForeignKey(nameof(SessionId))]
        public Session Session { get; set; }

        [Required]
        public int DogId { get; set; }

        [ForeignKey(nameof(DogId))]
        public Dog Dog { get; set; }

        [Required]
        public bool IsRegistered { get; set; } = false;

        [Required]
        public bool HasParticipated { get; set; } = false;

        public string? Comment { get; set; }
    }
}
