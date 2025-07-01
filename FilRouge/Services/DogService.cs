using FilRouge.Data;
using FilRouge.Models;
using FilRouge.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FilRouge.Services
{
    public class DogService : IDogService
    {
        private readonly ApplicationDbContext _context;

        public DogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public int GetAgeInMonths(Dog dog)
        {
            var today = DateTime.Today;
            int months = (today.Year - dog.BirthDate.Year) * 12 + today.Month - dog.BirthDate.Month;
            if (dog.BirthDate.Day > today.Day) months--;
            return months;
        }

        public async Task<List<Dog>> GetDogsForUserAsync(string identityUserId)
        {
            var person = await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == identityUserId);

            if (person == null)
                return new List<Dog>();

            return await _context.Dogs
                .Include(d => d.Breed)
                .Where(d => d.PersonId == person.PersonId)
                .ToListAsync();
        }

        public async Task<Dog?> GetDogByIdAsync(int dogId)
        {
            return await _context.Dogs
                .Include(d => d.Breed)
                .Include(d => d.Person)
                .FirstOrDefaultAsync(d => d.DogId == dogId);
        }
    }
}
