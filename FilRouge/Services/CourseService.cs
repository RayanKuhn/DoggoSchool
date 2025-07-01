using FilRouge.Data;
using FilRouge.Models;
using FilRouge.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FilRouge.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsCreatedByAsync(int courseId, int personId)
        {
            return await _context.Courses
                .AnyAsync(c => c.CourseId == courseId && c.PersonId == personId);
        }

        public async Task<bool> ValidateCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return false;

            course.IsValidatedByAdmin = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Course>> GetCoursesForTeacherAsync(int personId)
        {
            return await _context.Courses
                .Include(c => c.Person)
                .Where(c => c.PersonId == personId)
                .ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Person)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<List<Course>> GetValidatedCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Person)
                .Where(c => c.IsValidatedByAdmin)
                .ToListAsync();
        }

        public async Task<List<Course>> GetValidatedCoursesForTeacherAsync(int personId)
        {
            return await _context.Courses
                .Include(c => c.Person)
                .Where(c => c.PersonId == personId && c.IsValidatedByAdmin)
                .ToListAsync();
        }

        public async Task<Course?> GetValidatedCourseByIdAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Person)
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.IsValidatedByAdmin);
        }

        public async Task<List<Course>> GetCompatibleCoursesForDogAsync(Dog dog)
        {
            var today = DateTime.Today;
            var ageInMonths = ((today.Year - dog.BirthDate.Year) * 12) + today.Month - dog.BirthDate.Month;
            if (dog.BirthDate.Day > today.Day) ageInMonths--;

            return await _context.Courses
                .Include(c => c.Person)
                .Where(c => c.IsValidatedByAdmin &&
                    (!c.AgeMin.HasValue || ageInMonths >= c.AgeMin) &&
                    (!c.AgeMax.HasValue || ageInMonths <= c.AgeMax) &&
                    (!c.HeightMin.HasValue || dog.Height >= c.HeightMin) &&
                    (!c.HeightMax.HasValue || dog.Height <= c.HeightMax) &&
                    (!c.WeightMin.HasValue || dog.Weight >= c.WeightMin) &&
                    (!c.WeightMax.HasValue || dog.Weight <= c.WeightMax)
                )
                .ToListAsync();
        }


    }
}
