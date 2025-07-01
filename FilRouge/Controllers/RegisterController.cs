using FilRouge.Data;
using FilRouge.Models;
using FilRouge.Services;
using FilRouge.Services.Interfaces;
using FilRouge.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilRouge.Controllers
{
    [Authorize(Roles = "User")]
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICourseService _courseService;
        private readonly IRegisterService _registerService;

        public RegisterController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ICourseService courseService, IRegisterService registerService)
        {
            _context = context;
            _userManager = userManager;
            _courseService = courseService;
            _registerService = registerService;
        }

        [HttpGet]
        public async Task<IActionResult> AvailableCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            var dogs = await _context.Dogs
                .Include(d => d.Breed)
                .Include(d => d.Person)
                .Where(d => d.Person.IdentityUserId == user.Id)
                .ToListAsync();

            return View(dogs);
        }


        [HttpGet]
        public async Task<IActionResult> GetCoursesForDog(int dogId)
        {
            var user = await _userManager.GetUserAsync(User);
            var dog = await _context.Dogs
                .Include(d => d.Breed)
                .Include(d => d.Person)
                .FirstOrDefaultAsync(d => d.DogId == dogId);

            if (dog == null || dog.Person.IdentityUserId != user.Id)
                return Forbid();

            var compatibleCourses = await _registerService.GetCoursesNotAlreadyRegisteredAsync(dog);

            var courseViewModels = compatibleCourses.Select(c => new CourseForUserViewModel
            {
                CourseId = c.CourseId,
                Name = c.Name,
                DurationInMinutes = c.DurationInMinutes,
                TeacherName = c.Person.FirstName + " " + c.Person.LastName,
                AgeMin = c.AgeMin,
                AgeMax = c.AgeMax,
                HeightMin = c.HeightMin,
                HeightMax = c.HeightMax,
                WeightMin = c.WeightMin,
                WeightMax = c.WeightMax
            }).ToList();

            var viewModel = new AvailableCoursesForDogViewModel
            {
                Dog = dog,
                CompatibleCourses = courseViewModels
            };

            return PartialView("_CourseListPartial", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterToCourse(int dogId, int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _registerService.TryRegisterDogToCourseAsync(dogId, courseId, user.Id);

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("AvailableCourses", new { dogId });
            }

            TempData["Success"] = "Votre inscription au cours a bien été enregistrée.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var registrations = await _registerService.GetActiveRegistrationsForPersonAsync(person.PersonId);

            var viewModels = registrations.Select(r => new RegistrationViewModel
            {
                RegisterId = r.RegisterId,
                DogName = r.Dog.Name,
                CourseName = r.Course.Name,
                TeacherName = $"{r.Course.Person.FirstName} {r.Course.Person.LastName}",
                RegistrationDate = r.RegistrationDate,
                Status = r.Status,
                DogId = r.DogId,
                CourseId = r.CourseId
            }).ToList();

            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Quit(int registerId)
        {
            var user = await _userManager.GetUserAsync(User);

            var result = await _registerService.QuitRegistrationAsync(registerId, user.Id);

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("Index");
            }

            TempData["Success"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }






    }

}
