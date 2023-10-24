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

                // Check if a user request with the given NIC exists.
                var existingUserRequest = await userRequestCollection.Find(x => x.NIC == userRequest.NIC).FirstOrDefaultAsync();
                if (existingUserRequest != null && existingUserRequest.Status == "PENDING")
                {
                    // Handle the case where the user does not exist.
                    return (false, "UserExistingValidation", "User Request Alreay Exist"); // Return null values if user not found
                }

                if (existingUser.Status == Status.Deleted)
                {
                    return ( false, "UserStatusValidation", "User Is Deleted");
                }
                else if (existingUser.IsActive)
                {
                    return (false, "UserIsACtiveValidation", "User Is Active, Can not request a activation request for already active user");
                }

                userRequest.Remark = "Request to enable respective user profile";
                userRequest.Status = "PENDING";
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
        public async Task<UserRequest> UpdateUserRequestAsync(string id, UserRequest userRequest)
        {
            try
            { 
                // Ensure that user ID is not modified during update.
                userRequest.Id = id;

                // Retrieve the current user (assuming you have a way to identify the current user, such as from claims).
                var currentUser = GetLoggedInUser();

                if (currentUser == null)
                {
                    throw new Exception("Current user not found.");
                }

                // Set updated timestamp and updated by fields.
                userRequest.UpdatedOn = DateTime.UtcNow;
                userRequest.UpdatedBy = currentUser.Id; 

                if(userRequest.Status == "APPROVED")
                {
                    // Exclude NIC  fields from update.
                    var updateDefinitionUserDetails = Builders<UserDetails>.Update
                       .Set(u => u.IsActive, true)
                       .Set(u => u.UpdatedOn, userRequest.UpdatedOn)
                       .Set(u => u.UpdatedBy, userRequest.UpdatedBy);

                    // Implement user update logic here.
                    await userCollection.UpdateOneAsync(u => u.NIC == userRequest.NIC, updateDefinitionUserDetails);
                }

                // Exclude NIC  fields from update.
                var updateDefinition = Builders<UserRequest>.Update  
                   .Set(u => u.Status, userRequest.Status) 
                   .Set(u => u.UpdatedOn, userRequest.UpdatedOn)
                   .Set(u => u.UpdatedBy, userRequest.UpdatedBy);

                 // Implement user update logic here.
                 await userRequestCollection.UpdateOneAsync(u => u.Id == id, updateDefinition);

                // Fetch and return the updated user details.
                var updatedUserRequest = await userRequestCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
                return updatedUserRequest; 
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
        public async Task<string> DeleteUserRequestAsync(string id)
        {
            try
            {
                // Check if the user with the specified ID exists.
                var existingUserRequest = await userRequestCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
                if (existingUserRequest == null)
                {
                    // Handle the case where the user does not exist.
                    return "User request not found."; // Return a custom message
                }

                // Implement user deletion logic here.
                await userRequestCollection.DeleteOneAsync(x => x.Id == id);

                return "User request deleted successfully."; // Return a custom message upon successful deletion
            }
            catch (Exception ex)
            {
                // Handle other exceptions and return appropriate responses.
                throw ex;
            }
        }

        public UserDetails GetLoggedInUser()
        {
            try
            {
                // Retrieve the current HTTP context and user identity.
                var httpContext = httpContextAccessor.HttpContext;
                var user = httpContext?.User;

                // Check if the user is authenticated.
                if (user != null && user.Identity.IsAuthenticated)
                {
                    // Retrieve the user's NIC (used as email) from the claims.
                    var nicClaim = user.FindFirst(ClaimTypes.Name);

                    if (nicClaim != null)
                    {
                        var nic = nicClaim.Value;

                        // Query the database to get the user based on NIC (email).
                        var existingUser = userCollection.Find(x => x.NIC == nic).FirstOrDefault();

                        if (existingUser != null)
                        {
                            // Return the user details.
                            return existingUser;
                        }
                    }
                }

                // Handle the case where the user is not authenticated or not found.
                throw new Exception("User not authenticated or not found.");
            }
            catch (Exception ex)
            {
                // Handle other exceptions and return appropriate responses.
                throw ex;
            }
        }

    }
}
