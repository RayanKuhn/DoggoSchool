using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilRouge.Models
{
    public class Session
    {
        [Key]
        public int SessionId { get; set; }

        [Required]
        public DateOnly CourseDate { get; set; }

        [Required]
        public TimeOnly CourseHour { get; set; }

        [Required]
        public int MembersMax { get; set; }

        public int MembersRegistered { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }


    }
}
