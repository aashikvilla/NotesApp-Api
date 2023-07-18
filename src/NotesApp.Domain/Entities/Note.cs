using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotesApp.Domain.Entities
{
    public class Note
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Title")]
        public string Title { get; set; }

        [BsonElement("Description")]
        public string Description { get; set; }

        [BsonElement("Priority")]
        public string Priority { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; }

        [BsonElement("UserId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
    }
}
