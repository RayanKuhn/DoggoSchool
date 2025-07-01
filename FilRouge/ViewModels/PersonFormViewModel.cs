using System.ComponentModel.DataAnnotations;

namespace FilRouge.ViewModels
{
    public class PersonFormViewModel
    {
        public int PersonId { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [Display(Name = "Prénom")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [Display(Name = "Nom")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Adresse obligatoire")]
        [Display(Name = "Adresse")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Date de naissance obligatoire")]
        [Display(Name = "Date de naissance")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Téléphone obligatoire")]
        [Display(Name = "Téléphone")]
        public string PhoneNumber { get; set; }
    }
}

