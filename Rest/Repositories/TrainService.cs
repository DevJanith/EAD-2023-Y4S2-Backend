/*
 * Filename: TrainService.cs
 * Author: Supun Dileepa
 * Date: October 8, 2023
 * Description: Include backend logic implementation for all the TrainService methods.
 */

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rest.Configurations;
using Rest.Entities;
using Rest.Models;

namespace Rest.Repositories
{
    public class TrainService : ITrainService
    {
        private readonly IMongoCollection<Train> trainCollection;
        // Initialize MongoDB Data
        public TrainService(IOptions<ProductDBSettings> productDatabasSettings)
        {
            var mongoClient = new MongoClient(productDatabasSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(productDatabasSettings.Value.DatabaseName);
            trainCollection = mongoDatabase.GetCollection<Train>(productDatabasSettings.Value.TrainCollectionName);
        }
        // Get Train List
        public async Task<List<Train>> TrainListAsync()
        {
            return await trainCollection.Find(_ => true).ToListAsync();
        }
        // Get Train Details by Given ID
        public async Task<Train> GetTrainDetailByIdAsync(string trainId)
        {
            return await trainCollection.Find(x => x.Id == trainId).FirstOrDefaultAsync();
        }
        // Add New Train
        public async Task AddTrainAsync(Train trainDetails)
        {
            await trainCollection.InsertOneAsync(trainDetails);
        }

        // Update Train by Item ID
        public async Task UpdateTrainAsync(string trainId, Train trainDetails)
        {
            await trainCollection.ReplaceOneAsync(x => x.Id == trainId, trainDetails);
        }
        // Delete Train
        public async Task DeleteTrainAsync(string trainId)
        {
            await trainCollection.DeleteOneAsync(x => x.Id == trainId);
        }
    }
}
