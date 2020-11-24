using System.Collections.Generic;
using System.Threading.Tasks;
using AnimalAPI.Models;

namespace AnimalAPI.Services
{
    public interface IAnimalService
    {
        Task<List<Dog>> GetAllDogsAsync();
        Task<List<Cat>> GetAllCatsAsync();
        Task InsertDogAsync(Dog dog);
        Task InsertCatAsync(Cat cat);
    }
}