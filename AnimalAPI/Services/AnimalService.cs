using System.Collections.Generic;
using System.Threading.Tasks;
using AnimalAPI.Database;
using AnimalAPI.Models;
using MongoDB.Driver;

namespace AnimalAPI.Services
{

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
            // you always need a filter when using mongodb
            var findCursor = await Animals.OfType<Dog>().FindAsync(x => true);
            return await findCursor.ToListAsync();
        }

        public async Task<List<Cat>> GetAllCatsAsync()
        {
            // you always need a filter when using mongodb
            var findCursor = await Animals.OfType<Cat>().FindAsync(x => true);
            return await findCursor.ToListAsync();
        }

        public async Task InsertDogAsync(Dog dog)
        {
            // as soon as you insert an entity you automatically get an id filled in.
            await Animals.InsertOneAsync(dog);
        }

        public async Task InsertCatAsync(Cat cat)
        {
            // as soon as you insert an entity you automatically get an id filled in.
            await Animals.InsertOneAsync(cat);
        }
    }
}