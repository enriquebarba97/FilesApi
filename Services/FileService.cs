using System.IO;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using FilesApi.Models;

namespace FilesApi.Services
{

    public class FileService
    {
        private readonly IGridFSBucket bucket;


        public FileService(IFilesDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            bucket = new GridFSBucket(database);
        }

        public void Create(string filename, IFormFile file)
        {
            var id = bucket.UploadFromStream(filename, file.OpenReadStream());
        }

        public byte[] GetFile(string filename)
        {
            var options = new GridFSDownloadByNameOptions
            {
                Revision = -1
            };
            var file = bucket.DownloadAsBytesByName(filename,options);
            return file;
        }
    }
}