using System.IO;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using FilesApi.Models;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;

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

        public FileMetadata Create(string username, IFormFile file)
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

            var currentData = GetFileMetadata(username, filename);

            if(currentData == null){
                currentData = new FileMetadata(filename,username);
                filesMetadata.InsertOne(currentData);
            }else{
                var update = Builders<FileMetadata>.Update.Set("revisions", currentData.revisions + 1);

                filesMetadata.UpdateOne(fm => fm.id == currentData.id, update);
            }
            return currentData;
        }

        public List<FileMetadata> GetOwnFiles(string username)
        {
            return filesMetadata.Find(fm => fm.owner == username).ToList();
        }

        public List<FileMetadata> GetSharedFiles(string username)
        {
            return filesMetadata.Find(fm => fm.sharedWith.Contains(username)).ToList();
        }
        public FileMetadata GetFileMetadata(string owner, string filename)
        {
            return filesMetadata.Find<FileMetadata>(fd =>fd.filename == filename && fd.owner == owner).FirstOrDefault();
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
                    Sort = sort
                };
            var cursor = bucket.Find(filter, findOptions);
            
            var filesFound = cursor.ToList();
            var fileInfo = filesFound.FirstOrDefault();

            if(fileInfo != null){
                var file = bucket.DownloadAsBytes(fileInfo.Id);
                return file;
            }

            return null;
        }

        public void DeleteFile(string username, string filename)
        {
            var currentData = filesMetadata.Find<FileMetadata>(fd =>fd.filename == filename && fd.owner == username).FirstOrDefault();

            var filter = Builders<GridFSFileInfo>.Filter.And(
                Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["owner"], username),
                Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, filename)
            );

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var findOptions = new GridFSFindOptions
                {
                    Sort = sort
                };
            var cursor = bucket.Find(filter, findOptions);
            
            var filesFound = cursor.ToList();

            foreach(var fileInfo in filesFound){
                bucket.Delete(fileInfo.Id);
            }

            filesMetadata.DeleteOne(fm => fm.id == currentData.id);

        }

        public FileMetadata UpdateShares(string username, string filename, string share)
        {
            var currentData = filesMetadata.Find<FileMetadata>(fd =>fd.filename == filename && fd.owner == username).FirstOrDefault();

            if(currentData == null)
                return null;

            var update = Builders<FileMetadata>.Update.AddToSet("sharedWith", share);

            filesMetadata.UpdateOne(fm => fm.id == currentData.id, update);
            currentData.sharedWith.Add(share);

            return currentData;

        }
    }
}