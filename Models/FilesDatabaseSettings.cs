namespace FilesApi.Models
{
    public class FilesDatabaseSettings : IFilesDatabaseSettings
    {
        public string UsersCollectionName { get; set; }
        public string FilesMetadataCollectionName { get; set;}
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IFilesDatabaseSettings
    {
        string UsersCollectionName { get; set; }
        string FilesMetadataCollectionName { get; set;}
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}