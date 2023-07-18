using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotesApp.Domain.Entities
{

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }      

        [BsonElement("FirstName")]
        public string FirstName { get; set; }

        [BsonElement("LastName")]
        public string LastName { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }

    }
}
