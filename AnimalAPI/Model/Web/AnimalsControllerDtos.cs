using System.ComponentModel.DataAnnotations;

namespace AnimalAPI.Models
{
    // You can also group classes inside of a file. Not really best practice but if you have lots of small
    // DTOs this is actually more readable. Do not do this for complex objects and try to group them by domain. 
    public class CreateDogDto
    {
        [Required]
        public string Name { get; set; }
        public bool PottyTrained { get; set; }
        public bool Barks { get; set; }
    }

    public class CreateCatDto
    {
        [Required]
        public string Name { get; set; }
        public bool Hisses { get; set; }
    }

    public class AnimalLinks
    {
        public AnimalLinks(string self)
        {
            Self = self;
        }
        public string Self { get; set; }
    }

    public class ViewCatDto
    {
        public ViewCatDto()
        {

        }

        public ViewCatDto(Cat cat, AnimalLinks meta)
        {
            Name = cat.Name;
            Id = cat.Id;
            Hisses = cat.Hisses;
            Meta = meta;
        }

        public AnimalLinks Meta { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public bool Hisses { get; set; }
    }

    public class ViewDogDto
    {
        public ViewDogDto()
        {

        }

        public ViewDogDto(Dog dog, AnimalLinks meta)
        {
            Name = dog.Name;
            Id = dog.Id;
            Barks = dog.Barks;
            PottyTrained = dog.PottyTrained;
            Meta = meta;
        }

        public AnimalLinks Meta { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public bool Barks { get; set; }
        public bool PottyTrained { get; set; }
    }
}