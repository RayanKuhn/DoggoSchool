using FilRouge.Models;

namespace FilRouge.Services.Interfaces
{
    public interface ISessionService
    {
        bool CanTeacherEdit(Session session, int personId);
        Task<Session?> GetSessionByIdAsync(int sessionId);
        Task<List<Session>> GetSessionsByCourseAsync(int courseId);
        Task<bool> IsSessionTimeConflictingAsync(int teacherId, DateOnly date, TimeOnly hour, int durationInMinutes, int? sessionIdToIgnore = null);
    }
}