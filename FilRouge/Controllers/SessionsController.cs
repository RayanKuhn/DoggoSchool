using FilRouge.Data;
using FilRouge.Models;
using FilRouge.Services;
using FilRouge.Services.Interfaces;
using FilRouge.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FilRouge.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class SessionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICourseService _courseService;
        private readonly ISessionService _sessionService;


        public SessionsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ICourseService courseService, ISessionService sessionService)
        {
            _context = context;
            _userManager = userManager;
            _courseService = courseService;
            _sessionService = sessionService;

        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            List<Session> sessions;

            if (User.IsInRole("Admin"))
            {
                sessions = await _context.Sessions
                    .Include(s => s.Course)
                    .OrderBy(s => s.CourseDate)
                    .ToListAsync();
            }
            else
            {
                sessions = await _context.Sessions
                    .Include(s => s.Course)
                    .Where(s =>
                        s.Course.PersonId == person.PersonId &&
                        s.CourseDate >= DateOnly.FromDateTime(DateTime.Today)
                    )
                    .OrderBy(s => s.CourseDate)
                    .ToListAsync();
            }


            return View(sessions);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var viewModel = new SessionFormViewModel();

            List<Course> courses;

            if (User.IsInRole("Admin"))
            {
                courses = await _courseService.GetValidatedCoursesAsync();
            }

            else
            {
                courses = await _courseService.GetValidatedCoursesForTeacherAsync(person.PersonId);

            }

            viewModel.AvailableCourses = courses
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = $"{c.Name} - {c.Person.FirstName} {c.Person.LastName} - {c.DurationInMinutes} min"
                }).ToList();

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create(SessionFormViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                var courses = User.IsInRole("Admin")
                    ? await _context.Courses.Where(c => c.IsValidatedByAdmin).ToListAsync()
                    : await _context.Courses
                        .Where(c => c.PersonId == person.PersonId && c.IsValidatedByAdmin)
                        .ToListAsync();

                viewModel.AvailableCourses = courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseId.ToString(),
                        Text = c.Name
                    }).ToList();

                return View(viewModel);
            }

            var selectedCourse = await _courseService.GetCourseByIdAsync(viewModel.CourseId);
            if (selectedCourse == null)
                return NotFound();

            if (User.IsInRole("Teacher") && selectedCourse.PersonId != person.PersonId)
                return Forbid();

            var session = new Session
            {
                CourseDate = viewModel.CourseDate,
                CourseHour = viewModel.CourseHour,
                MembersMax = viewModel.MembersMax,
                MembersRegistered = 0,
                CourseId = viewModel.CourseId
            };

            if (User.IsInRole("Teacher"))
            {
                var hasConflict = await _sessionService.IsSessionTimeConflictingAsync(
                    person.PersonId,
                    viewModel.CourseDate,
                    viewModel.CourseHour,
                    selectedCourse.DurationInMinutes
                );

                if (hasConflict)
                {
                    ModelState.AddModelError("", "Vous avez déjà une session qui chevauche cet horaire.");
                    return View(viewModel);
                }
            }

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            if (viewModel.CourseId != 0)
            {
                return RedirectToAction("ByCourse", new { courseId = viewModel.CourseId });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var session = await _context.Sessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null)
                return NotFound();

            if (User.IsInRole("Teacher") && !_sessionService.CanTeacherEdit(session, person.PersonId))
                return Forbid();

            _context.Sessions.Remove(session);
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

            var session = await _sessionService.GetSessionByIdAsync(id);


            if (session == null)
                return NotFound();

            if (User.IsInRole("Teacher") && !_sessionService.CanTeacherEdit(session, person.PersonId))
                return Forbid();

            var viewModel = new SessionFormViewModel
            {
                SessionId = session.SessionId,
                CourseId = session.CourseId,
                CourseDate = session.CourseDate,
                CourseHour = session.CourseHour,
                MembersMax = session.MembersMax,
                MembersRegistered = session.MembersRegistered
            };

            List<Course> courses;
            if (User.IsInRole("Admin"))
            {
                courses = await _courseService.GetValidatedCoursesAsync();

            }
            else
            {
                courses = await _courseService.GetValidatedCoursesForTeacherAsync(person.PersonId);

            }

            viewModel.AvailableCourses = courses
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = $"{c.Name} - {c.Person.FirstName} {c.Person.LastName} - {c.DurationInMinutes} min",
                    Selected = c.CourseId == session.CourseId
                }).ToList();

            return View("Create", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(SessionFormViewModel viewModel)
        {
            if (viewModel.SessionId == null)
                return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var session = await _sessionService.GetSessionByIdAsync(viewModel.SessionId.Value);


            if (session == null)
                return NotFound();

            if (User.IsInRole("Teacher") && !_sessionService.CanTeacherEdit(session, person.PersonId))
                return Forbid();


            if (!ModelState.IsValid)
            {
                List<Course> courses;
                if (User.IsInRole("Admin"))
                {
                    courses = await _courseService.GetValidatedCoursesAsync();
                }
                else
                {
                    courses = await _courseService.GetValidatedCoursesForTeacherAsync(person.PersonId);
                }


                viewModel.AvailableCourses = courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseId.ToString(),
                        Text = $"{c.Name} - {c.Person.FirstName} {c.Person.LastName} - {c.DurationInMinutes} min",
                        Selected = c.CourseId == viewModel.CourseId
                    }).ToList();

                return View("Create", viewModel);
            }

            if (User.IsInRole("Teacher"))
            {
                var selectedCourse = await _courseService.GetCourseByIdAsync(viewModel.CourseId);

                if (selectedCourse == null)
                    return NotFound();

                var hasConflict = await _sessionService.IsSessionTimeConflictingAsync(
                    person.PersonId,
                    viewModel.CourseDate,
                    viewModel.CourseHour,
                    selectedCourse.DurationInMinutes,
                    session.SessionId // pour exclure cette session si on l'édite
                );

                if (hasConflict)
                {
                    ModelState.AddModelError("", "Vous avez déjà une session qui chevauche cet horaire.");
                    List<Course> courses = await _courseService.GetValidatedCoursesForTeacherAsync(person.PersonId);

                    viewModel.AvailableCourses = courses.Select(c => new SelectListItem
                    {
                        Value = c.CourseId.ToString(),
                        Text = $"{c.Name} - {c.Person.FirstName} {c.Person.LastName} - {c.DurationInMinutes} min",
                        Selected = c.CourseId == viewModel.CourseId
                    }).ToList();

                    return View("Create", viewModel);
                }
            }


            session.CourseDate = viewModel.CourseDate;
            session.CourseHour = viewModel.CourseHour;
            session.MembersMax = viewModel.MembersMax;
            session.CourseId = viewModel.CourseId;

            await _context.SaveChangesAsync();
            return RedirectToAction("ByCourse", new { courseId = viewModel.CourseId });

        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet]
        public async Task<IActionResult> ByCourse(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course == null)
                return NotFound();

            if (User.IsInRole("Teacher") && course.PersonId != person.PersonId)
                return Forbid();

            var sessions = await _sessionService.GetSessionsByCourseAsync(courseId);

            if (User.IsInRole("Teacher"))
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                sessions = sessions.Where(s => s.CourseDate >= today).ToList();
            }

            ViewBag.Course = course;
            return View(sessions);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateFromCourse(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var course = await _courseService.GetValidatedCourseByIdAsync(courseId);

            if (course == null)
                return NotFound();

            if (User.IsInRole("Teacher") && course.PersonId != person.PersonId)
                return Forbid();

            var viewModel = new SessionFormViewModel
            {
                CourseId = courseId,
                MembersRegistered = 0,
                AvailableCourses = new List<SelectListItem>
        {
            new SelectListItem
            {
                Value = course.CourseId.ToString(),
                Text = $"{course.Name} - {course.Person.FirstName} {course.Person.LastName} - {course.DurationInMinutes} min",
                Selected = true
            }
        }
            };

            ViewBag.CourseLocked = true;
            return View("Create", viewModel);
        }

    }

}
