using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimalAPI.Models;
using AnimalAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AnimalAPI.Controllers
{
    [ApiController]
    [Produces("application/json")]
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

        ///<summary>
        /// Get all cats
        ///</summary>
        [HttpGet("/cats")]
        public async Task<IActionResult> GetCats() => Ok(await GetAllCats());

        ///<summary>
        /// Get all dogs
        ///</summary>
        [HttpGet("/dogs")]
        public async Task<IActionResult> GetDogs() => Ok(await GetAllDogs());

        ///<summary>
        /// Get a single dog by id.
        ///</summary>
        ///<param name="id">The primary key of the dog object.</param>
        [HttpGet("/dogs/{id}")]
        public async Task<IActionResult> GetDog(string id)
        {
            var dogs = await GetAllDogs();
            var dog = dogs.FirstOrDefault(x => x.Id == id);
            if (dog == null)
            {
                return NotFound();
            }
            return Ok(dog);
        }

        ///<summary>
        /// Get a single cat by id.
        ///</summary>
        ///<param name="id">The primary key of the cat object.</param>
        [HttpGet("/cats/{id}")]
        public async Task<IActionResult> GetCat(string id)
        {
            var cats = await GetAllCats();
            var cat = cats.FirstOrDefault(x => x.Id == id);
            if (cat == null)
            {
                return NotFound();
            }
            return Ok(cat);
        }

        ///<summary>
        /// Creates a new dog
        ///</summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST /dogs
        ///     {
        ///        "Name": "Jeff",
        ///        "PottyTrained": true,
        ///        "Barks": true
        ///     }
        /// </remarks>
        [HttpPost("/dogs")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        ///<summary>
        /// Create a new cat
        ///</summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST /cats
        ///     {
        ///        "Name": "Jeff",
        ///        "Hisses": true,
        ///     }
        /// </remarks>        
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
