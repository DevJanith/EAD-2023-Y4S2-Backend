using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Microsoft.VisualBasic;

namespace Rest.Models
{

    public class Reservation
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ReservedCount { get; set; }
        public DateTime ReservationDate { get; set; }
        public string ReservationStatus { get; set; }  // "PENDING, RESERVED, CANCELLED",
        public decimal Amount { get; set; }
        public string? ScheduleId { get; set; }

    }
}
