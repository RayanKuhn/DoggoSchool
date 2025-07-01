using Microsoft.AspNetCore.Http;
using FilRouge.Models;
using FilRouge.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FilRouge.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FilRouge.Services;
using FilRouge.Services.Interfaces;
namespace FilRouge.Controllers
{
    public class CoursesController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ICourseService _courseService;
        public CoursesController(UserManager<IdentityUser> userManager, ApplicationDbContext context, ICourseService courseService)
        {
            _userManager = userManager;
            _context = context;
            _courseService = courseService;
        }


        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var identityUserId = user.Id;

            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == identityUserId);

            if (person == null)
                return Unauthorized();

            List<Course> courses;

            if (User.IsInRole("Admin"))
            {
                courses = await _context.Courses
                    .Include(c => c.Person)
                    .ToListAsync();
            }
            else
            {
                courses = await _courseService.GetCoursesForTeacherAsync(person.PersonId);

            }

            return View(courses);
        }

        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var identityUserId = user.Id;

            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == identityUserId);

            if (person == null)
                return Unauthorized();

            var viewModel = new CourseFormViewModel();

            if (User.IsInRole("Admin"))
            {
                var teachers = await _context.Persons
                    .Include(p => p.IdentityUser)
                    .ToListAsync();

                var teacherList = new List<SelectListItem>();

                foreach (var t in teachers)
                {
                    if (await _userManager.IsInRoleAsync(t.IdentityUser, "Teacher"))
                    {
                        teacherList.Add(new SelectListItem
                        {
                            Value = t.PersonId.ToString(),
                            Text = $"{t.FirstName} {t.LastName}"
                        });
                    }
                }

                viewModel.AvailablePersons = teacherList;
                viewModel.IsValidatedByAdmin = true;
            }
            else if (User.IsInRole("Teacher"))
            {
                viewModel.PersonId = person.PersonId;
                viewModel.IsValidatedByAdmin = false; 
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Create(CourseFormViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    var teachers = await _context.Persons
                        .Include(p => p.IdentityUser)
                        .Where(p => _userManager.IsInRoleAsync(p.IdentityUser, "Teacher").Result)
                        .ToListAsync();

                    viewModel.AvailablePersons = teachers
                        .Select(t => new SelectListItem
                        {
                            Value = t.PersonId.ToString(),
                            Text = $"{t.FirstName} {t.LastName}"
                        }).ToList();
                }

                return View(viewModel);
            }

            var course = new Course
            {
                Name = viewModel.Name,
                DurationInMinutes = viewModel.DurationInMinutes,
                AgeMin = viewModel.AgeMin,
                AgeMax = viewModel.AgeMax,
                HeightMin = viewModel.HeightMin,
                HeightMax = viewModel.HeightMax,
                WeightMin = viewModel.WeightMin,
                WeightMax = viewModel.WeightMax,
                IsValidatedByAdmin = User.IsInRole("Admin") ? true : false
            };

            if (User.IsInRole("Admin"))
            {
                course.PersonId = viewModel.PersonId;
            }
            else if (User.IsInRole("Teacher"))
            {
                course.PersonId = person.PersonId;
            }

            try
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erreur lors de l'enregistrement : " + ex.Message);

                if (User.IsInRole("Admin"))
                {
                    var teachers = await _context.Persons
                        .Include(p => p.IdentityUser)
                        .Where(p => _userManager.IsInRoleAsync(p.IdentityUser, "Teacher").Result)
                        .ToListAsync();

                    viewModel.AvailablePersons = teachers
                        .Select(t => new SelectListItem
                        {
                            Value = t.PersonId.ToString(),
                            Text = $"{t.FirstName} {t.LastName}"
                        }).ToList();
                }

                return View(viewModel);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Validate(int id)
        {
            var success = await _courseService.ValidateCourseAsync(id);
            if (!success)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var course = await _context.Courses.FindAsync(id);

            if (course == null)
                return NotFound();

            if (User.IsInRole("Teacher"))
            {
                var isOwner = await _courseService.IsCreatedByAsync(course.CourseId, person.PersonId);
                if (!isOwner)
                    return Forbid();
            }


            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            if (User.IsInRole("Teacher"))
            {
                var isOwner = await _courseService.IsCreatedByAsync(course.CourseId, person.PersonId);
                if (!isOwner)
                    return Forbid();
            }


            var viewModel = new CourseFormViewModel
            {
                CourseId = course.CourseId,
                Name = course.Name,
                DurationInMinutes = course.DurationInMinutes,
                AgeMin = course.AgeMin,
                AgeMax = course.AgeMax,
                HeightMin = course.HeightMin,
                HeightMax = course.HeightMax,
                WeightMin = course.WeightMin,
                WeightMax = course.WeightMax,
                PersonId = course.PersonId,
                IsValidatedByAdmin = course.IsValidatedByAdmin
            };

            if (User.IsInRole("Admin"))
            {
                var teachers = await _context.Persons
                    .Include(p => p.IdentityUser)
                    .ToListAsync();

                var teacherList = new List<SelectListItem>();

                foreach (var t in teachers)
                {
                    if (await _userManager.IsInRoleAsync(t.IdentityUser, "Teacher"))
                    {
                        teacherList.Add(new SelectListItem
                        {
                            Value = t.PersonId.ToString(),
                            Text = $"{t.FirstName} {t.LastName}",
                            Selected = t.PersonId == course.PersonId
                        });
                    }
                }

                viewModel.AvailablePersons = teacherList;
            }

            return View("Create", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(CourseFormViewModel viewModel)
        {
            if (viewModel.CourseId == null)
                return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var course = await _courseService.GetCourseByIdAsync(viewModel.CourseId.Value);
            if (course == null)
                return NotFound();

            if (User.IsInRole("Teacher"))
            {
                var isOwner = await _courseService.IsCreatedByAsync(course.CourseId, person.PersonId);
                if (!isOwner)
                    return Forbid();
            }


            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    var teachers = await _context.Persons
                        .Include(p => p.IdentityUser)
                        .ToListAsync();

                    viewModel.AvailablePersons = new List<SelectListItem>();

                    foreach (var t in teachers)
                    {
                        if (await _userManager.IsInRoleAsync(t.IdentityUser, "Teacher"))
                        {
                            viewModel.AvailablePersons.Add(new SelectListItem
                            {
                                Value = t.PersonId.ToString(),
                                Text = $"{t.FirstName} {t.LastName}",
                                Selected = t.PersonId == viewModel.PersonId
                            });
                        }
                    }
                }

                return View("Create", viewModel);
            }

            course.Name = viewModel.Name;
            course.DurationInMinutes = viewModel.DurationInMinutes;
            course.AgeMin = viewModel.AgeMin;
            course.AgeMax = viewModel.AgeMax;
            course.HeightMin = viewModel.HeightMin;
            course.HeightMax = viewModel.HeightMax;
            course.WeightMin = viewModel.WeightMin;
            course.WeightMax = viewModel.WeightMax;

            if (User.IsInRole("Admin"))
            {
                course.PersonId = viewModel.PersonId;
                course.IsValidatedByAdmin = viewModel.IsValidatedByAdmin;
            }
            else if (User.IsInRole("Teacher"))
            {
                course.IsValidatedByAdmin = false;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
