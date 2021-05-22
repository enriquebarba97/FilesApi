using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FilesApi.Models
{
public class FileMetadata{

        public FileMetadata(string _filename, string _owner)
        {
            filename = _filename;
            owner = _owner;
            revisions = 1;
            sharedWith = new List<string>();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set;}
        public string filename { get; set;}
        public int revisions { get; set;}
        public string owner { get; set;}
        public List<string> sharedWith { get; set;}
    }
}