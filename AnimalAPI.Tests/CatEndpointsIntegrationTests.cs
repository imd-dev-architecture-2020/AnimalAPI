using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AnimalAPI.Models;
using FizzWare.NBuilder;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AnimalAPI.Tests
{
    public class CatEndpointsIntegrationTests : BaseAnimalIntegrationTest
    {
        [Test]
        public async Task GetCats_EndpointReturnsSomeData()
        {
            // Arrange
            AddDefaultAnimals<Cat>(4);
            AddDefaultAnimals<Dog>(3);
            var client = Factory.CreateClient();

            // Act
            var response = await client.GetAsync("cats");
            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var body = await response.Content.ReadAsStringAsync();
            var deserializedResponseCount = JsonConvert.DeserializeObject<List<ViewCatDto>>(body).Count();
            deserializedResponseCount.Should().Be(4);
        }

        [Test]
        public async Task PersistCat()
        {
            var toInsert = Builder<CreateCatDto>.CreateNew().Build();

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toInsert));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var client = Factory.CreateClient();

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
            var toInsert = Builder<CreateCatDto>.CreateNew().Build();

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toInsert));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var client = Factory.CreateClient();

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
        public async Task GetSingleCat_IdDoesNotExist()
        {
            AddDefaultAnimals<Cat>(200);
            AddDefaultAnimals<Dog>(200);
            // Arrange
            var client = Factory.CreateClient();
            // Act
            var response = await client.GetAsync($"/cats/blabla");
            // Assert
            response.StatusCode.Should().Be(404);
        }

        [Test]
        public async Task PersistCat_EmptyNameThrowsError()
        {
            var toInsert = new CreateCatDto();

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toInsert));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var client = Factory.CreateClient();

            // Act
            var responsePost = await client.PostAsync("cats", byteContent);
            responsePost.StatusCode.Should().Be(400);
        }
    }
}
