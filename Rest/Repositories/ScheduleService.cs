/*
 * Filename: ScheduleService.cs
 * Author: Himasha Ranaweera
 * ID Number : IT20251000
 * Date: October 8, 2023
 * Description: Include backend logic implementation for all the ScheduleService methods.
 */



using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rest.Configurations;
using Rest.Entities;
using Rest.Models;

namespace Rest.Repositories
{
    public class ScheduleService : IScheduleService
    {
        private readonly IMongoCollection<Schedule> scheduleCollection;
        private readonly IMongoCollection<Reservation> reservationCollection;


        public class ReservedCountExceedsTotalSeatsException : Exception
        {
            public ReservedCountExceedsTotalSeatsException(string message) : base(message)
            {
            }
        }

        public class ScheduleNotFoundException : Exception
        {
            public ScheduleNotFoundException(string message) : base(message)
            {
            }
        }
        // Initialize MongoDB Data
        public ScheduleService(IOptions<ProductDBSettings> productDatabaseSettings)
        {
            var mongoClient = new MongoClient(productDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(productDatabaseSettings.Value.DatabaseName);
            scheduleCollection = mongoDatabase.GetCollection<Schedule>(productDatabaseSettings.Value.ScheduleCollectionName);
            reservationCollection = mongoDatabase.GetCollection<Reservation>(productDatabaseSettings.Value.ReservationCollectionName);
        }
        // Get All Schedules
        public async Task<List<Schedule>> ScheduleListAsync()
        {
            return await scheduleCollection.Find(_ => true).ToListAsync();
        }
        // Get Schedule by ID
        public async Task<Schedule> GetScheduleDetailByIdAsync(string scheduleId)
        {
            return await scheduleCollection.Find(x => x.Id == scheduleId).FirstOrDefaultAsync();
        }
        // Add new Schedule
        public async Task AddScheduleAsync(Schedule scheduleDetails)
        {
            await scheduleCollection.InsertOneAsync(scheduleDetails);
        }
        // Update Schedule by ID
        public async Task UpdateScheduleAsync(string scheduleId, Schedule scheduleDetails)
        {
            await scheduleCollection.ReplaceOneAsync(x => x.Id == scheduleId, scheduleDetails);
        }
        // Delete Schdule by ID 
        public async Task DeleteScheduleAsync(string scheduleId)
        {
            await scheduleCollection.DeleteOneAsync(x => x.Id == scheduleId);
        }

    
        // Add Reservation to Exsting schedule.
        // This method takes Schedule ID and Reservation details as input.
        // Create new Reservation and add it to Reservation collection and Add this item to reservatins [] in Schedule as well.
        public async Task AddReservationToScheduleAsync(string scheduleId, Reservation reservation)
        {
            var existingSchedule = await scheduleCollection.Find(x => x.Id == scheduleId).FirstOrDefaultAsync();
            if (existingSchedule == null)
            {
                // Schedule not found
                throw new ScheduleNotFoundException("Schedule not found.");
            }

            var totalSeats = existingSchedule.train?.TotalSeats ?? 0;
            var reservedCount = existingSchedule.reservations?
                .Where(r => r.ReservationStatus == "RESERVED")
                .Sum(r => r.ReservedCount) ?? 0;

            if (reservedCount + reservation.ReservedCount > totalSeats)
            {
                // Error: ReservedCount exceeds TotalSeats
                throw new ReservedCountExceedsTotalSeatsException("Reserved Count exceeds available seat count.");
            }

            reservation.ScheduleId = scheduleId;

            await reservationCollection.InsertOneAsync(reservation);
            var filter = Builders<Schedule>.Filter.Eq(x => x.Id, scheduleId);
            var update = Builders<Schedule>.Update.Push(x => x.reservations, reservation);

            await scheduleCollection.UpdateOneAsync(filter, update);
        }

        // Update Schedule Details
        public async Task UpdateScheduleAsync(Schedule schedule)
        {
            var filter = Builders<Schedule>.Filter.Eq(x => x.Id, schedule.Id);
            var update = Builders<Schedule>.Update.Set(x => x.reservations, schedule.reservations);

            await scheduleCollection.UpdateOneAsync(filter, update);
        }

       
        // Get all Schedules by Status and Filter schedules which schedule date is greater than or equal today
        public async Task<List<Schedule>> GetSchedulesByStatusAsync(string status)
        {
            // Get the current time
            DateTime currentTime = DateTime.Now;

            var filter = Builders<Schedule>.Filter.And(
                Builders<Schedule>.Filter.Eq(x => x.Status, status),
                Builders<Schedule>.Filter.Gte(x => x.StartDatetime, currentTime)
            );


            var schedules = await scheduleCollection.Find(filter).ToListAsync();

            return schedules;
        }

        // Get all schedules where shcedule date is greater than or equal to today
        public async Task<List<Schedule>> GetIncomingSchedules()
        {
            // Get the current time
            DateTime currentTime = DateTime.Now;

            // Calculate the end date which is 30 days from now
            DateTime endDate = currentTime.AddDays(30);

            var filter = Builders<Schedule>.Filter.And(
                Builders<Schedule>.Filter.Gte(x => x.StartDatetime, currentTime),
                Builders<Schedule>.Filter.Lte(x => x.StartDatetime, endDate)
            );

            var schedules = await scheduleCollection.Find(filter).ToListAsync();

            return schedules;
        }
    }
}