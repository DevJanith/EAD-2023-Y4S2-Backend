using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Rest.Entities
{
    public class UserDetails
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("NIC")]
        public string NIC { get; set; } // National Identification Card

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("ContactNo")]
        public string ContactNo { get; set; }

        [BsonElement("Salutation")]
        public string Salutation { get; set; }

        [BsonElement("FirstName")]
        public string FirstName { get; set; }

        [BsonElement("LastName")]
        public string LastName { get; set; }

        [BsonElement("UserRole")]
        public string UserRole { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; }

        [BsonElement("ActivationStatus")]
        public string ActivationStatus { get; set; }

        [BsonElement("CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [BsonElement("UpdatedOn")]
        public DateTime UpdatedOn { get; set; }

        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; }

        [BsonElement("UpdatedBy")]
        public string UpdatedBy { get; set; }
    }
}
