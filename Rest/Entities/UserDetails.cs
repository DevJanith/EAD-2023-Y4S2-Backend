using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace Rest.Entities
{
    public class UserDetails
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Salutation")]
        public Salutation? Salutation { get; set; }

        [BsonElement("FirstName")]
        public string? FirstName { get; set; }

        [BsonElement("LastName")]
        public string? LastName { get; set; }

        [BsonElement("ContactNumber")]
        public string? ContactNumber { get; set; }

        [BsonElement("Email")] 
        public string Email { get; set; }

        [BsonElement("NIC")] 
        public string NIC { get; set; }

        [BsonElement("UserType")]
        public UserType UserType { get; set; }

        [BsonElement("Status")]
        public Status Status { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; }

        [BsonElement("IsSysGenPassword")]
        public bool IsSysGenPassword { get; set; }

        [BsonElement("Password")]
        public string? Password { get; set; }

        [BsonElement("CreatedOn")] 
        public DateTime CreatedOn { get; set; }

        [BsonElement("UpdatedOn")] 
        public DateTime UpdatedOn { get; set; }

        [BsonElement("CreatedBy")]
        public string? CreatedBy { get; set; }

        [BsonElement("UpdatedBy")]
        public string? UpdatedBy { get; set; }
    }

    public enum UserType
    {
        Admin, // {key : 0 , value: "Admin"}
        BackOffice, // {key : 1 , value: "BackOffice"}
        TravelAgent, // {key : 2 , value: "TravelAgent"}
        User // {key : 3 , value: "User"}
    }

    public enum Status
    {
        Default, // {key : 0 , value: "Default"}
        New, // {key : 1 , value: "New"}
        Approved, // {key : 2 , value: "Approved"}
        Deleted // {key : 3 , value: "Deleted"}
    }

    public enum Salutation
    {
        Mr,// {key : 0 , value: "Mr"}
        Mrs,// {key : 1 , value: "Mrs"}
        Miss,// {key : 2 , value: "Miss"}
        Dr,// {key : 3 , value: "Dr"}
        Prof,// {key : 4 , value: "Prof"}
        Rev,// {key : 5 , value: "Rev"}
        Other// {key : 6 , value: "Other"}
    }
}
