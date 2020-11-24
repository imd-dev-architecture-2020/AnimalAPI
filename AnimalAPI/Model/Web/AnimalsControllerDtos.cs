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
}