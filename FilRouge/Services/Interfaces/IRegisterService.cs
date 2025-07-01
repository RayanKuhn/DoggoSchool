using FilRouge.Models;

namespace FilRouge.Services.Interfaces
{
    public interface IRegisterService
    {
        bool CanRegisterDogToCourse(Dog dog, Course course);
        Task<List<Register>> GetActiveRegistrationsForPersonAsync(int personId);
        Task<List<Course>> GetCoursesNotAlreadyRegisteredAsync(Dog dog);
        Task<(bool Success, string? ErrorMessage)> QuitRegistrationAsync(int registerId, string identityUserId);
        Task<(bool Success, string? ErrorMessage)> TryRegisterDogToCourseAsync(int dogId, int courseId, string identityUserId);
    }
}