using FilRouge.Data;
using FilRouge.Models;
using FilRouge.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FilRouge.Controllers
{
    public class ParticipateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IParticipateService _participateService;

        public ParticipateController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IParticipateService participateService)
        {
            _context = context;
            _userManager = userManager;
            _participateService = participateService;
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AvailableSessionsSelector(int? dogId = null, int page = 0)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (person == null)
                return Unauthorized();

            var dogs = await _context.Dogs
                .Where(d => d.PersonId == person.PersonId)
                .ToListAsync();

            ViewBag.DogList = new SelectList(dogs, "DogId", "Name");

            return View("AvailableSessionsSelector");
        }



        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetSessionsForDog(int dogId, int page = 0)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            var dog = await _context.Dogs
                .Include(d => d.Person)
                .FirstOrDefaultAsync(d => d.DogId == dogId);

            if (person == null || dog == null || dog.PersonId != person.PersonId)
                return Forbid();

            var sessions = await _participateService.GetAvailableSessionsForDogAsync(dog, page);

            ViewBag.Dog = dog;
            ViewBag.CurrentWeek = page;

            return PartialView("_SessionListPartial", sessions);
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterToSession(int sessionId, int dogId, int page = 0)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            var dog = await _context.Dogs
                .FirstOrDefaultAsync(d => d.DogId == dogId);

            if (person == null || dog == null || dog.PersonId != person.PersonId)
                return Forbid();

            var session = await _context.Sessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null || session.MembersRegistered >= session.MembersMax)
                return BadRequest();

            // Check s’il est déjà inscrit à une autre session au même moment
            var dogSessions = await _context.Participations
                .Include(p => p.Session)
                .Where(p => p.DogId == dogId && p.IsRegistered)
                .ToListAsync();

            var sessionStart = session.CourseHour;
            var sessionEnd = sessionStart.AddMinutes(session.Course.DurationInMinutes);

            var conflit = dogSessions.Any(p =>
                p.Session.CourseDate == session.CourseDate &&
                (
                    sessionStart < p.Session.CourseHour.AddMinutes(session.Course.DurationInMinutes) &&
                    sessionEnd > p.Session.CourseHour
                )
            );

            if (conflit)
                return BadRequest("Conflit d’horaire avec une autre session.");

            var participation = new Participate
            {
                DogId = dogId,
                SessionId = sessionId,
                IsRegistered = true
            };

            session.MembersRegistered += 1;
            _context.Participations.Add(participation);
            await _context.SaveChangesAsync();

            return RedirectToAction("AvailableSessionsSelector", new { dogId = dogId, page = page });
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnregisterFromSession(int sessionId, int dogId, int page = 0)
        {
            var user = await _userManager.GetUserAsync(User);
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            var dog = await _context.Dogs
                .FirstOrDefaultAsync(d => d.DogId == dogId);

            if (person == null || dog == null || dog.PersonId != person.PersonId)
                return Forbid();

            var participation = await _context.Participations
                .FirstOrDefaultAsync(p => p.SessionId == sessionId && p.DogId == dogId && p.IsRegistered);

            if (participation == null)
                return NotFound();

            var session = await _context.Sessions.FindAsync(sessionId);
            session.MembersRegistered -= 1;

            _context.Participations.Remove(participation);
            await _context.SaveChangesAsync();

            return RedirectToAction("AvailableSessionsSelector", new { dogId = dogId, page = page });
        }
    }
}
