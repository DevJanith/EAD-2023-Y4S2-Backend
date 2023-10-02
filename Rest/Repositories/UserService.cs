using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rest.Configurations;
using Rest.Entities;

namespace Rest.Repositories
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<UserDetails> userCollection;

        public UserService(IOptions<ProductDBSettings> productDatabaseSettings)
        {
            var mongoClient = new MongoClient(productDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(productDatabaseSettings.Value.DatabaseName);
            userCollection = mongoDatabase.GetCollection<UserDetails>(productDatabaseSettings.Value.UserCollectionName);
        }

        public async Task<List<UserDetails>> GetUserListAsync()
        {
            return await userCollection.Find(_ => true).ToListAsync();
        }

        public async Task<UserDetails> GetUserDetailsByIdAsync(string userId)
        {
            return await userCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(UserDetails userDetails)
        {
            // Default user status should be "deactivated."
            userDetails.Status = "1";
            userDetails.IsActive = false;
            userDetails.ActivationStatus = "1";

            // Set created and updated timestamps.
            userDetails.CreatedOn = DateTime.UtcNow;
            userDetails.UpdatedOn = userDetails.CreatedOn;

            await userCollection.InsertOneAsync(userDetails);
        }

        public async Task UpdateUserAsync(string userId, UserDetails userDetails)
        {
            // Ensure that user ID is not modified during update.
            userDetails.Id = userId;

            // Set updated timestamp.
            userDetails.UpdatedOn = DateTime.UtcNow;

            await userCollection.ReplaceOneAsync(x => x.Id == userId, userDetails);
        }

        public async Task DeleteUserAsync(string userId)
        {
            await userCollection.DeleteOneAsync(x => x.Id == userId);
        }

        public async Task ActivateUserAsync(string userId)
        {
            // Activate the user.
            var update = Builders<UserDetails>.Update
                .Set(x => x.IsActive, true)
                .Set(x => x.Status, "3") // Set status to "approve"
                .Set(x => x.ActivationStatus, "1"); // Set activation status to "1" (activated)

            await userCollection.UpdateOneAsync(x => x.Id == userId, update);
        }

        public async Task RequestDeactivationAsync(string userId)
        {
            // Send a deactivation request to the back office.
            var update = Builders<UserDetails>.Update
                .Set(x => x.ActivationStatus, "2"); // Set activation status to "2" (deactivation requested)

            await userCollection.UpdateOneAsync(x => x.Id == userId, update);
        }
    }
}
