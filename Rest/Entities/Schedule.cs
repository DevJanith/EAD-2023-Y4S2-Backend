﻿/*
 * Filename: Schedule.cs
 * Author: Himasha Ranaweera
 * ID Number : IT20251000
 * Date: October 8, 2023
 * Description: Model class for Schedule document
 */


using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Rest.Models
{
    public class Schedule
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public DateTime StartDatetime { get; set; }
        public DateTime EndDatetime { get; set; }
        public decimal TicketPrice { get; set; }
        public string Status { get; set; } // "ACTIVE, CANCELLED,",
        public Train? train { get; set; }
        public List<Reservation>? reservations { get; set; }

    }
}
