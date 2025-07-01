using FilRouge.Models;

namespace FilRouge.Services.Interfaces
{
    public interface ICourseService
    {
        Task<List<Course>> GetCompatibleCoursesForDogAsync(Dog dog);
        Task<Course?> GetCourseByIdAsync(int courseId);
        Task<List<Course>> GetCoursesForTeacherAsync(int personId);
        Task<Course?> GetValidatedCourseByIdAsync(int courseId);
        Task<List<Course>> GetValidatedCoursesAsync();
        Task<List<Course>> GetValidatedCoursesForTeacherAsync(int personId);
        Task<bool> IsCreatedByAsync(int courseId, int personId);
        Task<bool> ValidateCourseAsync(int courseId);
    }
}