using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rest.Configurations;
using Rest.Entities;
using Rest.Models;

namespace Rest.Repositories
{
    public class ReservationService : IReservationService
    {
        private readonly IMongoCollection<Reservation> reservationCollection;

        public ReservationService(IOptions<ProductDBSettings> productDatabaseSettings)
        {
            var mongoClient = new MongoClient(productDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(productDatabaseSettings.Value.DatabaseName);
            reservationCollection = mongoDatabase.GetCollection<Reservation>("Reservation");
        }

        public async Task<List<Reservation>> ReservationListAsync()
        {
            return await reservationCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Reservation> GetReservationDetailByIdAsync(string reservationId)
        {
            return await reservationCollection.Find(x => x.Id == reservationId).FirstOrDefaultAsync();
        }

        public async Task AddReservationAsync(Reservation reservationDetails)
        {
            await reservationCollection.InsertOneAsync(reservationDetails);
        }

        public async Task UpdateReservationAsync(string reservationId, Reservation reservationDetails)
        {
            await reservationCollection.ReplaceOneAsync(x => x.Id == reservationId, reservationDetails);
        }

        public async Task DeleteReservationAsync(string reservationId)
        {
            await reservationCollection.DeleteOneAsync(x => x.Id == reservationId);
        }
    }
}