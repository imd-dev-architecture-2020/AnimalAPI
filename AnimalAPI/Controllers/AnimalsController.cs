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
        [HttpGet("/cats", Name = nameof(GetCats))]
        public async Task<IActionResult> GetCats() => Ok(await GetAllCatsFromCacheAsync());

        ///<summary>
        /// Get all dogs
        ///</summary>
        [HttpGet("/dogs", Name = nameof(GetDogs))]
        public async Task<IActionResult> GetDogs() => Ok(await GetAllDogsFromCacheAsync());

        ///<summary>
        /// Get a single dog by id.
        ///</summary>
        ///<param name="id">The primary key of the dog object.</param>
        [HttpGet("/dogs/{id}", Name = nameof(GetDog))]
        public async Task<IActionResult> GetDog(string id)
        {
            var dogs = await GetAllDogsFromCacheAsync();
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
        [HttpGet("/cats/{id}", Name = nameof(GetCat))]
        public async Task<IActionResult> GetCat(string id)
        {
            var cats = await GetAllCatsFromCacheAsync();
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
        [HttpPost("/dogs", Name = nameof(CreateDog))]
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
            // Debatable if you want to do a fetch from the cache or a fetch directly from the db. This all depends on your configuration
            var returningDog = (await GetAllDogsFromCacheAsync()).FirstOrDefault(x => x.Id == dog.Id);
            return base.CreatedAtRoute(nameof(GetDog), new { id = dog.Id }, returningDog);
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
        [HttpPost("/cats", Name = nameof(CreateCats))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCats([FromBody] CreateCatDto catDto)
        {
            var cat = new Cat
            {
                Hisses = catDto.Hisses,
                Name = catDto.Name
            };
            await _animalService.InsertCatAsync(cat);
            _memoryCache.Remove(CacheKeys.AllCats);
            var returningCat = (await GetAllCatsFromCacheAsync()).FirstOrDefault(x => x.Id == cat.Id);
            return base.CreatedAtRoute(nameof(GetCat), new { id = cat.Id }, returningCat);
        }

        private Task<List<ViewDogDto>> GetAllDogsFromCacheAsync() =>
            _memoryCache.GetOrCreateAsync(CacheKeys.AllDogs, entry => GetAllDogsAsViewDto());

        private async Task<List<ViewDogDto>> GetAllDogsAsViewDto()
        {
            var dogs = await _animalService.GetAllDogsAsync();
            return dogs.Select(x => new ViewDogDto(x, new AnimalLinks(Url.Link(nameof(GetDog), new { id = x.Id })))).ToList();
        }

        private Task<List<ViewCatDto>> GetAllCatsFromCacheAsync() => 
            _memoryCache.GetOrCreateAsync(CacheKeys.AllCats, entry => GetAllCatsAsViewDto());

        private async Task<List<ViewCatDto>> GetAllCatsAsViewDto()
        {
            var cats = await _animalService.GetAllCatsAsync();
            return cats.Select(x => new ViewCatDto(x, new AnimalLinks(Url.Link(nameof(GetCat), new { id = x.Id })))).ToList();
        }
    }
}
