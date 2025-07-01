using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilRouge.Models
{
    public class Person
    {
        [Key]
        public int PersonId { get; set; }

        //[Required]
        //public string EmailAdress { get; set; }

        //[Required]
        //public string PasswordHash { get; set; }

        [Required]
        public Role Role { get; set; } // Admin / Teacher / User

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string IdentityUserId { get; set; }

        [ForeignKey("IdentityUserId")]
        public IdentityUser IdentityUser { get; set; }
    }
}
