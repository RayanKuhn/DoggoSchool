using FilRouge.Data;
using FilRouge.Models;
using FilRouge.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilRouge.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (person == null)
                return NotFound();

            return View(person);
        }


        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var userId = _userManager.GetUserId(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (person == null) return NotFound();

            var vm = new PersonFormViewModel
            {
                PersonId = person.PersonId,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Address = person.Address,
                PhoneNumber = person.PhoneNumber,
                BirthDate = person.BirthDate
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(PersonFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var person = await _context.Persons.FindAsync(vm.PersonId);

            if (person == null)
                return NotFound();

            person.FirstName = vm.FirstName;
            person.LastName = vm.LastName;
            person.Address = vm.Address;
            person.PhoneNumber = vm.PhoneNumber;
            person.BirthDate = vm.BirthDate;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profil mis à jour avec succès !";

            return RedirectToAction("Index");
        }
    }
}

