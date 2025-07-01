using FilRouge.Data;
using FilRouge.Models;
using FilRouge.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FilRouge.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDogService _dogService;
        private readonly ICourseService _courseService;

        public RegisterService(ApplicationDbContext context, IDogService dogService, ICourseService courseService)
        {
            _context = context;
            _dogService = dogService;
            _courseService = courseService;
        }

        public bool CanRegisterDogToCourse(Dog dog, Course course)
        {
            int ageInMonths = _dogService.GetAgeInMonths(dog);

            return
                (!course.AgeMin.HasValue || ageInMonths >= course.AgeMin) &&
                (!course.AgeMax.HasValue || ageInMonths <= course.AgeMax) &&
                (!course.HeightMin.HasValue || dog.Height >= course.HeightMin) &&
                (!course.HeightMax.HasValue || dog.Height <= course.HeightMax) &&
                (!course.WeightMin.HasValue || dog.Weight >= course.WeightMin) &&
                (!course.WeightMax.HasValue || dog.Weight <= course.WeightMax);
        }

        public async Task<(bool Success, string? ErrorMessage)> TryRegisterDogToCourseAsync(int dogId, int courseId, string identityUserId)
        {
            var dog = await _context.Dogs
                .Include(d => d.Person)
                .FirstOrDefaultAsync(d => d.DogId == dogId);

            if (dog == null || dog.Person.IdentityUserId != identityUserId)
                return (false, "Ce chien ne vous appartient pas.");

            var course = await _context.Courses
                .Include(c => c.Person)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null || !course.IsValidatedByAdmin)
                return (false, "Ce cours est introuvable ou non validé.");

            if (!CanRegisterDogToCourse(dog, course))
                return (false, "Votre chien ne remplit pas les conditions pour ce cours.");

            bool alreadyRegistered = await _context.Registers
                .AnyAsync(r => r.DogId == dogId && r.CourseId == courseId && r.Status == Status.Registered);

            if (alreadyRegistered)
                return (false, "Ce chien est déjà inscrit à ce cours.");

            var register = new Register
            {
                DogId = dogId,
                CourseId = courseId,
                Status = Status.Registered,
                RegistrationDate = DateOnly.FromDateTime(DateTime.Today)
            };

            _context.Registers.Add(register);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<List<Register>> GetActiveRegistrationsForPersonAsync(int personId)
        {
            return await _context.Registers
                .Include(r => r.Dog)
                .Include(r => r.Course)
                    .ThenInclude(c => c.Person)
                .Where(r => r.Dog.PersonId == personId && r.Status == Status.Registered)
                .OrderByDescending(r => r.RegistrationDate)
                .ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> QuitRegistrationAsync(int registerId, string identityUserId)
        {
            var register = await _context.Registers
                .Include(r => r.Dog).ThenInclude(d => d.Person)
                .Include(r => r.Course).ThenInclude(c => c.Person)
                .FirstOrDefaultAsync(r => r.RegisterId == registerId);

            if (register == null || register.Dog.Person.IdentityUserId != identityUserId)
                return (false, "Accès interdit ou inscription introuvable.");

            if (register.Status != Status.Registered)
                return (false, "Cette inscription n'est pas active.");

            register.Status = Status.Quitted;
            await _context.SaveChangesAsync();

            return (true, $"Vous vous êtes désinscrit du cours « {register.Course.Name} ».");
        }

        public async Task<List<Course>> GetCoursesNotAlreadyRegisteredAsync(Dog dog)
        {
            var registeredCourseIds = await _context.Registers
                .Where(r => r.DogId == dog.DogId && r.Status == Status.Registered)
                .Select(r => r.CourseId)
                .ToListAsync();

            var compatibleCourses = await _courseService.GetCompatibleCoursesForDogAsync(dog);

            return compatibleCourses
                .Where(c => !registeredCourseIds.Contains(c.CourseId))
                .ToList();
        }

    }
}
