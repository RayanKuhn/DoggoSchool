using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using FilRouge.Models;
using System.Collections.Generic;
using System.Linq;

namespace FilRouge.ViewModels
{
    public class DogFormViewModel
    {
        public Dog Dog { get; set; }

        [BindNever]
        [ValidateNever]
        public List<Breed> BreedList { get; set; }

        [BindNever]
        [ValidateNever]
        public IEnumerable<SelectListItem> Races =>
            BreedList?.Select(b => new SelectListItem
            {
                Value = b.BreedId.ToString(),
                Text = b.BreedName.ToString()
            }) ?? new List<SelectListItem>();
    }
}
