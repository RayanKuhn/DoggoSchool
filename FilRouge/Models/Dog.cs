using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilRouge.Models
{
    public class Dog
    {
        [Key]
        public int DogId { get; set; }

        [Required]
        [Display(Name = "Nom du chien")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Date de naissance")]
        public DateTime BirthDate { get; set; }

        [Required]
        [Display(Name = "Taille (en cm)")]
        public int? Height { get; set; }

        [Required]
        [Range(0, 200)]
        [Display(Name = "Poids (en kg)")]
        public decimal? Weight { get; set; }

        [Required]
        [Display(Name = "Problèmes de santé")]
        public string HealthIssues { get; set; }

        [Required]
        public int PersonId { get; set; }

        [ForeignKey(nameof(PersonId))]
        public Person Person { get; set; }

        [Required]
        [Display(Name = "La race de votre chien")]
        public int BreedId { get; set; }

        [ForeignKey(nameof(BreedId))]
        public Breed Breed { get; set; }
    }
}
