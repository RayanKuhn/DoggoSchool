using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilRouge.Models
{
    public class Concerns
    {
        [Key]
        public int ConcernsId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }

        [Required]
        public int BreedId { get; set; }

        [ForeignKey(nameof(BreedId))]
        public Breed Breed { get; set; }
    }
}
