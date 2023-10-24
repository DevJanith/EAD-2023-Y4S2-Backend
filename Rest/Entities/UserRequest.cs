using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Rest.Entities
{
    public class UserRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } 

        [BsonElement("NIC")]
        public string NIC { get; set; }

        [BsonElement("Remark")]
        public string? Remark { get; set; }

        [BsonElement("CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [BsonElement("UpdatedOn")]
        public DateTime UpdatedOn { get; set; }

        [BsonElement("CreatedBy")]
        public string? CreatedBy { get; set; }

        [BsonElement("UpdatedBy")]
        public string? UpdatedBy { get; set; }
    }
}
