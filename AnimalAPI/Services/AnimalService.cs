using System.Collections.Generic;
using System.Threading.Tasks;
using AnimalAPI.Database;
using AnimalAPI.Models;
using MongoDB.Driver;

namespace AnimalAPI.Services
{
    public interface IAnimalService
    {
        Task<List<Dog>> GetAllDogsAsync();
        Task<List<Cat>> GetAllCatsAsync();
        Task PersistDogAsync(Dog dog);
        Task<Dog> GetDogById(string id);
    }

    public class AnimalService : IAnimalService
    {
        public IMongoCollection<Animal> Animals { get; }
        public MongoClient MongoClient { get; }

        public AnimalService(IAnimalDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            Animals = database.GetCollection<Animal>(settings.AnimalCollectionName);
            MongoClient = client;
        }

        public async Task<List<Dog>> GetAllDogsAsync()
        {
            var findCursor = await Animals.OfType<Dog>().FindAsync(x => true);
            return await findCursor.ToListAsync();
        }

        public async Task<List<Cat>> GetAllCatsAsync()
        {
            var findCursor = await Animals.OfType<Cat>().FindAsync(x => true);
            return await findCursor.ToListAsync();
        }

        public async Task PersistDogAsync(Dog dog)
        {
            await Animals.InsertOneAsync(dog);
        }

        public async Task<Dog> GetDogById(string id)
        {
            return (await Animals.OfType<Dog>().FindAsync(x => x.Id == id)).FirstOrDefault();
        }
    }
}