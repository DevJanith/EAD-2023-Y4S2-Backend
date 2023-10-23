/*
 * Filename: Train.cs
 * Author: Ridma Dilshan
 * ID Number : IT20005276
 * Date: October 8, 2023
 * Description: Model class for Train document
 */

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
        public string Status { get; set; } // ACTIVE, INACTIVE
        public string PublishStatus { get; set; } // PUBLISHED. UNPUBLISHED
        public int TotalSeats { get; set; }
      
    }
}
