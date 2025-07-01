using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FilRouge.ViewModels
{
    public class SessionFormViewModel
    {
        public int? SessionId { get; set; }

        [Required]
        [Display(Name = "Date du cours")]
        public DateOnly CourseDate { get; set; }

        [Required]
        [Display(Name = "Heure de début")]
        public TimeOnly CourseHour { get; set; }

        [Required]
        [Display(Name = "Nombre maximum de places disponibles")]
        public int MembersMax { get; set; }

        public int MembersRegistered { get; set; } = 0;

        [Required]
        [Display(Name = "Cours associé")]
        public int CourseId { get; set; }

        public List<SelectListItem>? AvailableCourses { get; set; }
    }
}
