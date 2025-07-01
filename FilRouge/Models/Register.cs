using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilRouge.Models
{
    public class Register
    {
        [Key]
        public int RegisterId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }

        [Required]
        public int DogId { get; set; }

        [ForeignKey(nameof(DogId))]
        public Dog Dog { get; set; }

        [Required]
        public Status Status { get; set; }

        [Required]
        public DateOnly RegistrationDate { get; set; }

        public int? Note { get; set; }

    }
}
