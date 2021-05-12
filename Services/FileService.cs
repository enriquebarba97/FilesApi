using System.IO;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using FilesApi.Models;
using System.Linq;

namespace FilesApi.Services
{

    public class FileService
    {
        private readonly IMongoCollection<FileMetadata> filesMetadata;
        private readonly IGridFSBucket bucket;


        public FileService(IFilesDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            filesMetadata = database.GetCollection<FileMetadata>(settings.FilesMetadataCollectionName);
            bucket = new GridFSBucket(database);
        }

        public void Create(string username, IFormFile file)
        {
            var options = new GridFSUploadOptions
            {
                ChunkSizeBytes = 64512, // 63KB
                Metadata = new BsonDocument
                {
                    { "owner", username }
                } 
            };

            var filename = file.FileName;
            
            var id = bucket.UploadFromStream(filename, file.OpenReadStream(), options);

            var currentData = filesMetadata.Find<FileMetadata>(fd =>fd.filename == filename && fd.owner == username).FirstOrDefault();

            if(currentData == null){
                var fileMetadata = new FileMetadata(filename,username);
                filesMetadata.InsertOne(fileMetadata);
            }else{
                var filter = Builders<FileMetadata>.Filter.Eq("_id", currentData.id);
                var update = Builders<FileMetadata>.Update.Set("revisions", currentData.revisions + 1);

                filesMetadata.UpdateOne(fm => fm.id == currentData.id, update);
            }
        }

        public byte[] GetFile(string username, string filename)
        {
            var filter = Builders<GridFSFileInfo>.Filter.And(
                Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["owner"], username),
                Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, filename)
            );

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var findOptions = new GridFSFindOptions
                {
                    Limit = 5,
                    Sort = sort
                };
            using (var cursor = bucket.Find(filter, findOptions))
            {
                var filesFound = cursor.ToList();
                var fileInfo = filesFound.FirstOrDefault();

                if(fileInfo != null){
                    var file = bucket.DownloadAsBytes(fileInfo.Id);
                    return file;
                }
            }

            return null;
        }
    }
}