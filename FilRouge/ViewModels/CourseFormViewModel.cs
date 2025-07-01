using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FilRouge.ViewModels
{
    public class CourseFormViewModel
    {
        public int? CourseId { get; set; }

        [Required]
        [Display(Name = "Nom du cours")]

        public string Name { get; set; }

        [Required]
        [Display(Name = "Durée (en minutes)")]
        public int DurationInMinutes { get; set; }

        [Display(Name = "Âge minimum (en mois)")]
        public int? AgeMin { get; set; }

        [Display(Name = "Âge maximum (en mois)")]
        public int? AgeMax { get; set; }

        [Display(Name = "Taille minimum (en cm)")]
        public int? HeightMin { get; set; }

        [Display(Name = "Taille maximum (en cm)")]
        public int? HeightMax { get; set; }

        [Display(Name = "Poids minimum (en kg)")]
        public decimal? WeightMin { get; set; }

        [Display(Name = "Poids maximum (en kg)")]
        public decimal? WeightMax { get; set; }

        public int PersonId { get; set; }

        [Display(Name = "Validé par l'Admin")]
        public bool IsValidatedByAdmin { get; set; }

        public List<SelectListItem>? AvailablePersons { get; set; }
    }
}
