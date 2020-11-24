using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AnimalAPI.Database;
using AnimalAPI.Models;
using AnimalAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AnimalAPI.Tests
{
    public class AnimalControllerIntegrationTests
    {
        private WebApplicationFactory<Startup> _factory;
        private AnimalService _animalService;

        [OneTimeSetUp]
        public void TestOneTimeSetup()
        {
            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, conf) =>
                    {
                        conf.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.test.json"));
                    });
                });


            var settings = (IAnimalDatabaseSettings)_factory.Services.GetService(typeof(IAnimalDatabaseSettings));
            _animalService = (AnimalService)_factory.Services.GetService(typeof(IAnimalService));
        }

        [TearDown]
        public void TestTearDown()
        {
            _animalService.MongoClient.DropDatabase("animal_tst");
        }

        [Test]
        public async Task GetCats_EndpointReturnsSomeData()
        {
            // Arrange
            var catsToCreate = new List<Cat> {
                new Cat { Hisses = true, Name = "Loki" },
                new Cat { Hisses = false, Name = "Felix" },
                new Cat { Hisses = true, Name = "Mario" },
                new Cat { Hisses = true, Name = "Esper" }
            };
            await _animalService.Animals.InsertManyAsync(catsToCreate);
            // since we don't need to check this data, throw it on the pile.
            var dogsToCreate = new List<Dog>
            {
                new Dog { Name = "Thor" },
                new Dog { Name = "Vadem" },
                new Dog { Name = "Vork" }
            };
            await _animalService.Animals.InsertManyAsync(dogsToCreate);
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("cats");
            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var body = await response.Content.ReadAsStringAsync();
            var deserializedResponseCount = JsonConvert.DeserializeObject<List<ViewCatDto>>(body).Count();
            deserializedResponseCount.Should().Be(catsToCreate.Count);
        }

        [Test]
        public async Task GetDogs_EndpointReturnsSomeData()
        {
            // Arrange
            var catsToCreate = new List<Cat> {
                new Cat { Hisses = true, Name = "Loki" },
                new Cat { Hisses = false, Name = "Felix" },
                new Cat { Hisses = true, Name = "Mario" },
                new Cat { Hisses = true, Name = "Esper" }
            };
            await _animalService.Animals.InsertManyAsync(catsToCreate);
            var dogsToCreate = new List<Dog>
            {
                new Dog { Name = "Thor" },
                new Dog { Name = "Vadem" },
                new Dog { Name = "Vork" }
            };
            await _animalService.Animals.InsertManyAsync(dogsToCreate);

            var client = _factory.CreateClient();
            // Act
            var response = await client.GetAsync("dogs");
            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var body = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<List<ViewDogDto>>(body).Count();
            deserializedResponse.Should().Be(dogsToCreate.Count());
        }

        [Test]
        public async Task PersistDog()
        {
            var dog = new CreateDogDto
            {
                Barks = true,
                Name = "Barbkbark",
                PottyTrained = false
            };

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dog));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsync("dogs", byteContent);
            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var body = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<ViewDogDto>(body);
            deserializedResponse.Name.Should().Be(dog.Name);
            deserializedResponse.PottyTrained.Should().Be(dog.PottyTrained);
            deserializedResponse.Barks.Should().Be(dog.Barks);
            deserializedResponse.Id.Should().NotBeNullOrWhiteSpace();

            deserializedResponse.Meta.Self.Should().Be($"http://localhost/dogs/{deserializedResponse.Id}");

            response.Headers.Location.Should().Be($"http://localhost/dogs/{deserializedResponse.Id}");
        }

        [Test]
        public async Task PersistCat()
        {
            var toInsert = new CreateCatDto
            {
                Hisses = true,
                Name = "miauwmiauw"
            };

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toInsert));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsync("cats", byteContent);
            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var body = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<ViewCatDto>(body);
            deserializedResponse.Name.Should().Be(toInsert.Name);
            deserializedResponse.Hisses.Should().Be(toInsert.Hisses);
            deserializedResponse.Id.Should().NotBeNullOrWhiteSpace();
            deserializedResponse.Meta.Self.Should().Be($"http://localhost/cats/{deserializedResponse.Id}");

            response.Headers.Location.Should().Be($"http://localhost/cats/{deserializedResponse.Id}");
        }

        [Test]
        public async Task GetSingleCat()
        {
            var toInsert = new CreateCatDto
            {
                Hisses = true,
                Name = "miauwmiauw"
            };

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toInsert));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var client = _factory.CreateClient();

            // Act
            var responsePost = await client.PostAsync("cats", byteContent);
            var response = await client.GetAsync(responsePost.Headers.Location);
            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var body = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<ViewCatDto>(body);
            deserializedResponse.Name.Should().Be(toInsert.Name);
            deserializedResponse.Hisses.Should().Be(toInsert.Hisses);
            deserializedResponse.Id.Should().NotBeNullOrWhiteSpace();
            deserializedResponse.Meta.Self.Should().Be($"http://localhost/cats/{deserializedResponse.Id}");
        }

        [Test]
        public async Task GetSingleDog()
        {
            var insertDog = new CreateDogDto
            {
                Barks = true,
                Name = "Barbkbark",
                PottyTrained = false
            };

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(insertDog));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var client = _factory.CreateClient();

            // Act
            var responsePost = await client.PostAsync("dogs", byteContent);
            var response = await client.GetAsync(responsePost.Headers.Location);
            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var body = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<ViewDogDto>(body);
            deserializedResponse.Name.Should().Be(insertDog.Name);
            deserializedResponse.PottyTrained.Should().Be(insertDog.PottyTrained);
            deserializedResponse.Barks.Should().Be(insertDog.Barks);
            deserializedResponse.Id.Should().NotBeNullOrWhiteSpace();
            deserializedResponse.Meta.Self.Should().Be($"http://localhost/dogs/{deserializedResponse.Id}");
        }

        [Test]
        public async Task GetSingleDog_IdDoesNotExist()
        {
            // Arrange
            var client = _factory.CreateClient();
            // Act
            var response = await client.GetAsync($"/dogs/blabla");
            // Assert
            response.StatusCode.Should().Be(404);
        }

        [Test]
        public async Task PersistDog_EmptyNameThrowsError()
        {
            var toInsert = new CreateDogDto();

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toInsert));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var client = _factory.CreateClient();

            // Act
            var responsePost = await client.PostAsync("dogs", byteContent);
            responsePost.StatusCode.Equals(400);
        }

        [Test]
        public async Task PersistCat_EmptyNameThrowsError()
        {
            var toInsert = new CreateCatDto();

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toInsert));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var client = _factory.CreateClient();

            // Act
            var responsePost = await client.PostAsync("cats", byteContent);
            responsePost.StatusCode.Equals(400);
        }
    }
}
