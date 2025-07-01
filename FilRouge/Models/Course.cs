using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilRouge.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int DurationInMinutes { get; set; }

        public int? AgeMin { get; set; }

        public int? AgeMax { get; set; }

        public int? HeightMin { get; set; }

        public int? HeightMax { get; set; }

        public decimal? WeightMin { get; set; }

        public decimal? WeightMax { get; set; }

        [Required]
        public bool IsValidatedByAdmin { get; set; } = false;

        [Required]
        public int PersonId { get; set; }

        [ForeignKey(nameof(PersonId))]
        public Person Person { get; set; }
    }
}

