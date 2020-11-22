using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimalAPI.Models;
using AnimalAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AnimalAPI.Controllers
{
    [ApiController]
    public class AnimalsController : ControllerBase
    {
        private readonly IAnimalService _animalService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AnimalsController> _logger;

        public AnimalsController(ILogger<AnimalsController> logger, IAnimalService animalService, IMemoryCache memoryCache)
        {
            _animalService = animalService;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        [HttpGet("/cats")]
        public async Task<IActionResult> GetCats()
        {
            var cats = await _memoryCache.GetOrCreateAsync(nameof(GetCats), entry =>
            {
                return _animalService.GetAllCatsAsync();
            });
            return Ok(cats);
        }

        [HttpGet("/dogs")]
        public async Task<IActionResult> GetDogs()
        {
            return Ok(await GetAllDogs());
        }

        [HttpGet("/dogs/{id}")]
        public async Task<IActionResult> GetDogs(string id)
        {
            var dogs = await GetAllDogs();
            var dog = dogs.FirstOrDefault(x => x.Id == id);
            if (dog == null)
            {
                return NotFound();
            }
            return Ok(dog);
        }

        [HttpPost("/dogs")]
        public async Task<IActionResult> CreateDog([FromBody] Dog dog)
        {
            await _animalService.PersistDogAsync(dog);
            _memoryCache.Remove(nameof(GetDogs));
            string uri = $"/dogs/{dog.Id}";
            return base.Created(uri, dog);
        }

        private async Task<List<Dog>> GetAllDogs()
        {
            return await _memoryCache.GetOrCreateAsync(nameof(GetDogs), entry =>
            {
                return _animalService.GetAllDogsAsync();
            });
        }

    }
}
