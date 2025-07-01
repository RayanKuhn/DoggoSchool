using FilRouge.Models;

namespace FilRouge.Services.Interfaces
{
    public interface IDogService
    {
        int GetAgeInMonths(Dog dog);
        Task<Dog?> GetDogByIdAsync(int dogId);
        Task<List<Dog>> GetDogsForUserAsync(string identityUserId);
    }
}