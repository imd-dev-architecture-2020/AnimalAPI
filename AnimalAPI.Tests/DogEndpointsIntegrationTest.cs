using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AnimalAPI.Models;
using FizzWare.NBuilder;
using FluentAssertions;
using MongoDB.Driver;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AnimalAPI.Tests
{
    public class DogEndpointsIntegrationTest : BaseAnimalIntegrationTest
    {
        [Test]
        public async Task GetDogs_EndpointReturnsSomeData()
        {
            // Arrange
            AddDefaultAnimals<Cat>(4);
            AddDefaultAnimals<Dog>(3);

            var client = Factory.CreateClient();
            // Act
            var response = await client.GetAsync("dogs");
            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var body = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<List<ViewDogDto>>(body).Count();
            deserializedResponse.Should().Be(3);
        }

        [Test]
        public async Task PersistDog()
        {
            var toInsert = Builder<CreateDogDto>.CreateNew().Build();

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toInsert));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var client = Factory.CreateClient();

            // Act
            var response = await client.PostAsync("dogs", byteContent);
            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var body = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<ViewDogDto>(body);
            deserializedResponse.Name.Should().Be(toInsert.Name);
            deserializedResponse.PottyTrained.Should().Be(toInsert.PottyTrained);
            deserializedResponse.Barks.Should().Be(toInsert.Barks);
            deserializedResponse.Id.Should().NotBeNullOrWhiteSpace();

            deserializedResponse.Meta.Self.Should().Be($"http://localhost/dogs/{deserializedResponse.Id}");

            response.Headers.Location.Should().Be($"http://localhost/dogs/{deserializedResponse.Id}");
        }

        [Test]
        public async Task GetSingleDog()
        {
            var toInsert = Builder<CreateDogDto>.CreateNew().Build();

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toInsert));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var client = Factory.CreateClient();

            // Act
            var responsePost = await client.PostAsync("dogs", byteContent);
            var response = await client.GetAsync(responsePost.Headers.Location);
            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var body = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<ViewDogDto>(body);
            deserializedResponse.Name.Should().Be(toInsert.Name);
            deserializedResponse.PottyTrained.Should().Be(toInsert.PottyTrained);
            deserializedResponse.Barks.Should().Be(toInsert.Barks);
            deserializedResponse.Id.Should().NotBeNullOrWhiteSpace();
            deserializedResponse.Meta.Self.Should().Be($"http://localhost/dogs/{deserializedResponse.Id}");
        }

        [Test]
        public async Task GetSingleDog_IdDoesNotExist()
        {
            AddDefaultAnimals<Dog>(200);
            AddDefaultAnimals<Cat>(200);

            // Arrange
            var client = Factory.CreateClient();
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
            var client = Factory.CreateClient();

            // Act
            var responsePost = await client.PostAsync("dogs", byteContent);
            responsePost.StatusCode.Should().Be(400);
        }
    }
}