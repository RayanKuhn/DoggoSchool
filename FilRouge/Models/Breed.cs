using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilRouge.Models
{
    public class Breed
    {
        [Key]
        public int BreedId { get; set; }

        [Required]
        public BreedName BreedName { get; set; }
    }
}
