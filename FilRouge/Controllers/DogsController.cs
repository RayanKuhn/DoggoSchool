using Microsoft.AspNetCore.Http;
using FilRouge.Models;
using FilRouge.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using FilRouge.ViewModels;
using FilRouge.Services;
using FilRouge.Services.Interfaces;

namespace FilRouge.Controllers
{
    [Authorize(Roles = "User")]
    public class DogsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IDogService _dogService;

        public DogsController(UserManager<IdentityUser> userManager, ApplicationDbContext context, IDogService dogService)
        {
            _userManager = userManager;
            _context = context;
            _dogService = dogService;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var dogs = await _dogService.GetDogsForUserAsync(user.Id);

            return View(dogs);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var breeds = await _context.Breeds.ToListAsync();

            var viewModel = new DogFormViewModel
            {
                Dog = new Dog(),
                BreedList = breeds
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DogFormViewModel viewModel)
        {
            ModelState.Remove("Dog.Person");
            ModelState.Remove("Dog.Breed");

            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
            {
                ModelState.AddModelError(string.Empty, "Aucun profil Person n'a été trouvé pour cet utilisateur.");
            }
            else
            {
                viewModel.Dog.PersonId = person.PersonId;
            }

            if (!ModelState.IsValid)
            {
                viewModel.BreedList = await _context.Breeds.ToListAsync();
                return View(viewModel);
            }

            try
            {
                var newDog = new Dog
                {
                    Name = viewModel.Dog.Name,
                    BirthDate = viewModel.Dog.BirthDate,
                    Height = viewModel.Dog.Height,
                    Weight = viewModel.Dog.Weight,
                    HealthIssues = viewModel.Dog.HealthIssues,
                    BreedId = viewModel.Dog.BreedId,
                    PersonId = viewModel.Dog.PersonId
                };

                _context.Dogs.Add(newDog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erreur lors de l'enregistrement: " + ex.Message);

                viewModel.BreedList = await _context.Breeds.ToListAsync();
                return View(viewModel);
            }
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var dog = await _context.Dogs.FirstOrDefaultAsync(d => d.DogId == id && d.PersonId == person.PersonId);

            if (dog == null)
                return NotFound();

            _context.Dogs.Remove(dog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var dog = await _context.Dogs
                .Include(d => d.Breed)
                .FirstOrDefaultAsync(d => d.DogId == id && d.PersonId == person.PersonId);

            if (dog == null)
                return NotFound();

            var breeds = await _context.Breeds.ToListAsync();

            var viewModel = new DogFormViewModel
            {
                Dog = dog,
                BreedList = breeds
            };

            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DogFormViewModel viewModel)
        {
            if (id != viewModel.Dog.DogId)
                return BadRequest();

            ModelState.Remove("Dog.Person");
            ModelState.Remove("Dog.Breed");

            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var existingDog = await _context.Dogs
                .FirstOrDefaultAsync(d => d.DogId == id && d.PersonId == person.PersonId);

            if (existingDog == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    existingDog.Name = viewModel.Dog.Name;
                    existingDog.BirthDate = viewModel.Dog.BirthDate;
                    existingDog.Height = viewModel.Dog.Height;
                    existingDog.Weight = viewModel.Dog.Weight;
                    existingDog.HealthIssues = viewModel.Dog.HealthIssues;
                    existingDog.BreedId = viewModel.Dog.BreedId;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Erreur lors de la mise à jour : " + ex.Message);
                }
            }

            // Recharge la liste des races pour la vue
            viewModel.BreedList = await _context.Breeds.ToListAsync();

            return View(viewModel);
        }



    }
}
