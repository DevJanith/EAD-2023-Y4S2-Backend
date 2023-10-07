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

        public ScheduleService(IOptions<ProductDBSettings> productDatabaseSettings)
        {
            var mongoClient = new MongoClient(productDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(productDatabaseSettings.Value.DatabaseName);
            scheduleCollection = mongoDatabase.GetCollection<Schedule>(productDatabaseSettings.Value.ScheduleCollectionName);
            reservationCollection = mongoDatabase.GetCollection<Reservation>(productDatabaseSettings.Value.ReservationCollectionName);
        }

        public async Task<List<Schedule>> ScheduleListAsync()
        {
            return await scheduleCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Schedule> GetScheduleDetailByIdAsync(string scheduleId)
        {
            return await scheduleCollection.Find(x => x.Id == scheduleId).FirstOrDefaultAsync();
        }

        public async Task AddScheduleAsync(Schedule scheduleDetails)
        {
            await scheduleCollection.InsertOneAsync(scheduleDetails);
        }

        public async Task UpdateScheduleAsync(string scheduleId, Schedule scheduleDetails)
        {
            await scheduleCollection.ReplaceOneAsync(x => x.Id == scheduleId, scheduleDetails);
        }

        public async Task DeleteScheduleAsync(string scheduleId)
        {
            await scheduleCollection.DeleteOneAsync(x => x.Id == scheduleId);
        }

       

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


        public async Task UpdateScheduleAsync(Schedule schedule)
        {
            var filter = Builders<Schedule>.Filter.Eq(x => x.Id, schedule.Id);
            var update = Builders<Schedule>.Update.Set(x => x.reservations, schedule.reservations);

            await scheduleCollection.UpdateOneAsync(filter, update);
        }

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
    }
}