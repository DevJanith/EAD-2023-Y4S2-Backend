using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rest.Configurations;
using Rest.Entities;
using Rest.util;
using System.Security.Claims;

namespace Rest.Repositories
{
    public class UserRequestService : IUserRequestService
    {
        private readonly IMongoCollection<UserRequest> userRequestCollection;
        private readonly IMongoCollection<UserDetails> userCollection;
        private readonly JwtSettings jwtSettings;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserRequestService(IOptions<ProductDBSettings> productDatabaseSettings, IOptions<JwtSettings> jwtSettings, IHttpContextAccessor httpContextAccessor)
        {
            var mongoClient = new MongoClient(productDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(productDatabaseSettings.Value.DatabaseName);
            userRequestCollection = mongoDatabase.GetCollection<UserRequest>(productDatabaseSettings.Value.UserRequestCollectionName);
            userCollection = mongoDatabase.GetCollection<UserDetails>(productDatabaseSettings.Value.UserCollectionName);
            this.jwtSettings = jwtSettings.Value;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<(List<UserRequest> UserRequests, int Total)> GetUserRequestsAsync(int page, int perPage, string direction)
        {
            try
            {
                // Validate the input parameters.
                if (page <= 0 || perPage <= 0)
                {
                    throw new ArgumentException("Invalid page or perPage parameters.");
                }

                var filterBuilder = Builders<UserRequest>.Filter;
                var filters = new List<FilterDefinition<UserRequest>>(); 

                // Initialize the filter with a default filter if no specific filters are added.
                FilterDefinition<UserRequest> filter = filterBuilder.Empty;

                if (filters.Count > 0)
                {
                    filter = filterBuilder.And(filters);
                }

                var sortBuilder = Builders<UserRequest>.Sort;
                var sort = direction.ToLowerInvariant() == "asc"
                    ? sortBuilder.Ascending(u => u.Id)
                    : sortBuilder.Descending(u => u.Id);

                var total = await userRequestCollection.CountDocumentsAsync(filter);
                var userRequests = await userRequestCollection
                    .Find(filter)
                    .Sort(sort)
                    .Skip((page - 1) * perPage)
                    .Limit(perPage)
                    .ToListAsync();

                return (userRequests, (int)total);
            }
            catch (ArgumentException ex)
            {
                // Handle invalid input arguments.
                throw ex;
            }
            catch (Exception ex)
            {
                // Handle other exceptions and return appropriate responses.
                throw ex;
            }
        }

        public async Task<(bool validUser, string stage, string desc)> CreateUserRequestAsync(UserRequest userRequest)
        {
            try
            {
                // Check if a user with the given NIC exists.
                var existingUser = await userCollection.Find(x => x.NIC == userRequest.NIC).FirstOrDefaultAsync();
                if (existingUser == null)
                {
                    // Handle the case where the user does not exist.
                    return (false, "UserExistingValidation", "User Does Not Exist"); // Return null values if user not found
                }

                userRequest.CreatedOn = DateTime.UtcNow;
                userRequest.UpdatedOn = userRequest.CreatedOn; 

                // Implement user creation logic here.
                await userRequestCollection.InsertOneAsync(userRequest);

                return (true, "", ""); // Return the token and user details upon successful sign-in
            }
            catch (FluentValidation.ValidationException ex)
            {
                // Handle validation errors and return appropriate responses.
                throw ex;
            }
            catch (Exception ex)
            {
                // Handle other exceptions and return appropriate responses.
                throw ex;
            }
        } 
    }
}
