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
            return Ok(await GetAllCats());
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
        public async Task<IActionResult> CreateDog([FromBody] CreateDogDto dogDto)
        {
            var dog = new Dog
            {
                Barks = dogDto.Barks,
                Name = dogDto.Name,
                PottyTrained = dogDto.PottyTrained
            };
            await _animalService.InsertDogAsync(dog);
            _memoryCache.Remove(CacheKeys.AllDogs);
            string uri = $"/dogs/{dog.Id}";
            return base.Created(uri, dog);
        }

        [HttpPost("/cats")]
        public async Task<IActionResult> CreateCats([FromBody] CreateCatDto catDto)
        {
            var cat = new Cat
            {
                Hisses = catDto.Hisses,
                Name = catDto.Name
            };
            await _animalService.InsertCatAsync(cat);
            _memoryCache.Remove(CacheKeys.AllCats);
            string uri = $"/cats/{cat.Id}";
            return base.Created(uri, cat);
        }

        private Task<List<Dog>> GetAllDogs() => _memoryCache.GetOrCreateAsync(CacheKeys.AllDogs, entry =>
            {
                return _animalService.GetAllDogsAsync();
            });


        private Task<List<Cat>> GetAllCats() => _memoryCache.GetOrCreateAsync(CacheKeys.AllCats, entry =>
        {
            return _animalService.GetAllCatsAsync();
        });
    }
}
