using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AnimalAPI.Database;
using AnimalAPI.Models;
using AnimalAPI.Services;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace AnimalAPI.Tests
{
    public abstract class BaseAnimalIntegrationTest
    {
        public WebApplicationFactory<Startup> Factory { get; private set; }
        public AnimalService AnimalService { get; private set; }

        [OneTimeSetUp]
        public void TestOneTimeSetup()
        {
            Factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, conf) =>
                    {
                        conf.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.test.json"));
                    });
                });


            var settings = (IAnimalDatabaseSettings)Factory.Services.GetService(typeof(IAnimalDatabaseSettings));
            AnimalService = (AnimalService)Factory.Services.GetService(typeof(IAnimalService));
        }

        
        [TearDown]
        public void TestTearDown()
        {
            AnimalService.MongoClient.DropDatabase("animal_tst");
        }

        public void AddDefaultAnimals<T>(int count) where T : Animal
        {
            // https://github.com/nbuilder/nbuilder
            var list = Builder<T>.CreateListOfSize(count).All().With(x => x.Id = null).Build();
            AnimalService.Animals.InsertMany(list);
        }
    }
}