using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AnimalAPI.Models
{
    public abstract class Animal
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}