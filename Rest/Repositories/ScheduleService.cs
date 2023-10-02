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

        public ScheduleService(IOptions<ProductDBSettings> productDatabaseSettings)
        {
            var mongoClient = new MongoClient(productDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(productDatabaseSettings.Value.DatabaseName);
            scheduleCollection = mongoDatabase.GetCollection<Schedule>("Schedule");
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
    }
}