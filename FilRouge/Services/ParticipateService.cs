using FilRouge.Data;
using FilRouge.Models;
using FilRouge.Services.Interfaces;
using FilRouge.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FilRouge.Services
{
    public class ParticipateService : IParticipateService
    {
        private readonly ApplicationDbContext _context;

        public ParticipateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SessionForDogViewModel>> GetAvailableSessionsForDogAsync(Dog dog, int weekOffset = 0)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var startDate = today.AddDays(7 * weekOffset);
            var endDate = startDate.AddDays(6);

            var registeredCourseIds = await _context.Registers
                .Where(r => r.DogId == dog.DogId && r.Status == Status.Registered)
                .Select(r => r.CourseId)
                .ToListAsync();

            // Sessions dans la période et dans ces cours
            var sessions = await _context.Sessions
                .Include(s => s.Course)
                    .ThenInclude(c => c.Person)
                .Where(s =>
                    registeredCourseIds.Contains(s.CourseId) &&
                    s.CourseDate >= startDate && s.CourseDate <= endDate)
                .OrderBy(s => s.CourseDate)
                .ThenBy(s => s.CourseHour)
                .ToListAsync();

            var sessionIdsWithParticipation = await _context.Participations
                .Where(p => p.DogId == dog.DogId && p.IsRegistered)
                .Select(p => p.SessionId)
                .ToListAsync();

            var sessionSet = sessionIdsWithParticipation.ToHashSet(); // pour accélérer les recherches


            return sessions.Select(s => new SessionForDogViewModel
            {
                SessionId = s.SessionId,
                CourseDate = s.CourseDate,
                CourseHour = s.CourseHour,
                DurationInMinutes = s.Course.DurationInMinutes,
                CourseName = s.Course.Name,
                TeacherName = s.Course.Person != null
                    ? $"{s.Course.Person.FirstName} {s.Course.Person.LastName}"
                    : "(Prof inconnu)",

                MembersMax = s.MembersMax,
                MembersRegistered = s.MembersRegistered,
                IsRegistered = sessionSet.Contains(s.SessionId),
                AgeMin = s.Course.AgeMin,
                AgeMax = s.Course.AgeMax,
                HeightMin = s.Course.HeightMin,
                HeightMax = s.Course.HeightMax,
                WeightMin = s.Course.WeightMin,
                WeightMax = s.Course.WeightMax,

                RegistrationDate = _context.Registers
                    .Where(r => r.CourseId == s.CourseId && r.DogId == dog.DogId && r.Status == Status.Registered)
                    .Select(r => r.RegistrationDate)
                    .FirstOrDefault(),
            }).ToList();
        }

    }
}
