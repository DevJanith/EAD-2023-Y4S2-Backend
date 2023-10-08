/*
 * Filename: ReservationService.cs
 * Author: Supun Dileepa
 * Date: October 8, 2023
 * Description: Include backend logic implementation for all the ReservationService methods.
 */


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

        // Initialize MongoDB Data
        public ReservationService(IOptions<ProductDBSettings> productDatabaseSettings)
        {
            var mongoClient = new MongoClient(productDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(productDatabaseSettings.Value.DatabaseName);
            reservationCollection = mongoDatabase.GetCollection<Reservation>(productDatabaseSettings.Value.ReservationCollectionName);
        }
        // Get All Reservations
        public async Task<List<Reservation>> ReservationListAsync()
        {
            return await reservationCollection.Find(_ => true).ToListAsync();
        }
        // Get Reservation by Item ID
        public async Task<Reservation> GetReservationDetailByIdAsync(string reservationId)
        {
            return await reservationCollection.Find(x => x.Id == reservationId).FirstOrDefaultAsync();
        }
        // Add new Reservation
        public async Task AddReservationAsync(Reservation reservationDetails)
        {
            await reservationCollection.InsertOneAsync(reservationDetails);
        }
        // Update Reservation Data
        public async Task UpdateReservationAsync(string reservationId, Reservation reservationDetails)
        {
            await reservationCollection.ReplaceOneAsync(x => x.Id == reservationId, reservationDetails);
        }
        // Delete Reservation
        public async Task DeleteReservationAsync(string reservationId)
        {
            await reservationCollection.DeleteOneAsync(x => x.Id == reservationId);
        }
        // Get Reservations of a User
        public async Task<List<Reservation>> GetReservationsByUserIdAsync(string userId)
        {
            var reservations = await reservationCollection.Find(r => r.UserId == userId).ToListAsync();
            return reservations;
        }
        // Get Reservations by given Status value
        public async Task<List<Reservation>> GetReservationsByStatusAsync(string status)
        {
         
            var filter = Builders<Reservation>.Filter.Eq(x => x.ReservationStatus, status);
            var reservations = await reservationCollection.Find(filter).ToListAsync();

            return reservations;
        }


    }
}