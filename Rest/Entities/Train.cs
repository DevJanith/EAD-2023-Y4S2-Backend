using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Rest.Models
{
    public class Train
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string TrainName { get; set; }
        public string TrainNumber { get; set; }
        public string AllocatedDriver { get; set; } 
        public string AllocatedGuard { get; set; }
        public int TotalSeats { get; set; }
      
    }
}
