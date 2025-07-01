using FilRouge.Data;
using FilRouge.Models;
using FilRouge.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FilRouge.Services
{
    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _context;

        public SessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Session?> GetSessionByIdAsync(int sessionId)
        {
            return await _context.Sessions
                .Include(s => s.Course)
                .ThenInclude(c => c.Person)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }

        public bool CanTeacherEdit(Session session, int personId)
        {
            return session.Course.PersonId == personId;
        }

        public async Task<List<Session>> GetSessionsByCourseAsync(int courseId)
        {
            return await _context.Sessions
                .Where(s => s.CourseId == courseId)
                .OrderBy(s => s.CourseDate)
                .ToListAsync();
        }

        public async Task<bool> IsSessionTimeConflictingAsync(int teacherId, DateOnly date, TimeOnly hour, int durationInMinutes, int? sessionIdToIgnore = null)
        {
            var endTime = hour.AddMinutes(durationInMinutes);

            var conflictingSession = await _context.Sessions
                .Include(s => s.Course)
                .Where(s => s.Course.PersonId == teacherId
                            && s.CourseDate == date
                            && (sessionIdToIgnore == null || s.SessionId != sessionIdToIgnore))
                .FirstOrDefaultAsync(s =>
                    s.CourseHour < endTime &&
                    s.CourseHour.AddMinutes(s.Course.DurationInMinutes) > hour
                );

            return conflictingSession != null;
        }


    }
}
