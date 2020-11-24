namespace AnimalAPI.Database
{
    public class AnimalDatabaseSettings : IAnimalDatabaseSettings
    {
        public string AnimalCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}