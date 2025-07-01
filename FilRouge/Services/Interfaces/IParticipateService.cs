using FilRouge.Models;
using FilRouge.ViewModels;

namespace FilRouge.Services.Interfaces
{
    public interface IParticipateService
    {
        Task<List<SessionForDogViewModel>> GetAvailableSessionsForDogAsync(Dog dog, int weekOffset = 0);
    }
}