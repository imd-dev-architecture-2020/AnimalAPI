namespace AnimalAPI.Database
{
    // 1:1 from appsettings.Development.json
    public interface IAnimalDatabaseSettings
    {
        string AnimalCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}