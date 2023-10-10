/*
 * Filename: UserService.cs
 * Author: Janith Gamage
 * ID Number : IT20402266
 * Date: October 8, 2023
 * Description: Include backend logic implementation for all the UserService methods.
 */

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rest.Configurations;
using Rest.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BCrypt.Net;
using FluentValidation;
using FluentValidation.Results;
using Rest.util;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Rest.Repositories
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<UserDetails> userCollection;
        private readonly JwtSettings jwtSettings;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserService(IOptions<ProductDBSettings> productDatabaseSettings, IOptions<JwtSettings> jwtSettings, IHttpContextAccessor httpContextAccessor)
        {
            var mongoClient = new MongoClient(productDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(productDatabaseSettings.Value.DatabaseName);
            userCollection = mongoDatabase.GetCollection<UserDetails>(productDatabaseSettings.Value.UserCollectionName);
            this.jwtSettings = jwtSettings.Value;
            this.httpContextAccessor = httpContextAccessor;
        }
        public async Task<(List<UserDetails> Users, int Total)> GetUsersAsync(int page, int perPage, string direction, Status? status, List<UserType> userTypes, bool? isActive)
        {
            try
            {
                // Validate the input parameters.
                if (page <= 0 || perPage <= 0)
                {
                    throw new ArgumentException("Invalid page or perPage parameters.");
                }

                var filterBuilder = Builders<UserDetails>.Filter;
                var filters = new List<FilterDefinition<UserDetails>>();

                // Add filters based on the provided criteria.
                if (status.HasValue)
                {
                    filters.Add(filterBuilder.Eq(u => u.Status, status.Value));
                }

                if (userTypes != null && userTypes.Any())
                {
                    filters.Add(filterBuilder.In(u => u.UserType, userTypes));
                }

                if (isActive.HasValue)
                {
                    filters.Add(filterBuilder.Eq(u => u.IsActive, isActive.Value));
                }

                // Initialize the filter with a default filter if no specific filters are added.
                FilterDefinition<UserDetails> filter = filterBuilder.Empty;

                if (filters.Count > 0)
                {
                    filter = filterBuilder.And(filters);
                }

                var sortBuilder = Builders<UserDetails>.Sort;
                var sort = direction.ToLowerInvariant() == "asc"
                    ? sortBuilder.Ascending(u => u.Id)
                    : sortBuilder.Descending(u => u.Id);

                var total = await userCollection.CountDocumentsAsync(filter);
                var users = await userCollection
                    .Find(filter)
                    .Sort(sort)
                    .Skip((page - 1) * perPage)
                    .Limit(perPage)
                    .ToListAsync();

                return (users, (int)total);
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
        public async Task CreateUserAsync(UserDetails userDetails)
        {
            try
            {
                // Validate the user details using FluentValidation.
                var validator = new UserDetailsValidator();
                FluentValidation.Results.ValidationResult validationResult = validator.Validate(userDetails);

                if (!validationResult.IsValid)
                {
                    // Handle validation errors and return appropriate responses.
                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }

                // Check if NIC (used as email) is unique before creating the user.
                if (await IsNicUnique(userDetails.NIC))
                {
                    // Check if the email is unique.
                    if (await IsEmailUnique(userDetails.Email))
                    {
                        // Encrypt the password before storing it.
                        userDetails.Password = BCrypt.Net.BCrypt.HashPassword(userDetails.Password);

                        // Retrieve the current user (assuming you have a way to identify the current user, such as from claims).
                        var currentUser = GetLoggedInUser();

                        if (currentUser == null)
                        {
                            throw new Exception("Current user not found.");
                        }

                        // Get the ID of the currently logged-in user or an appropriate source for createdBy.
                        string createdBy = currentUser.Id; // Replace with actual logic to obtain the user's ID

                        // Set createdBy, updatedBy, createdOn, and updatedOn fields.
                        userDetails.CreatedBy = createdBy;
                        userDetails.UpdatedBy = createdBy;
                        userDetails.CreatedOn = DateTime.UtcNow;
                        userDetails.UpdatedOn = userDetails.CreatedOn;
                        userDetails.IsSysGenPassword = true;

                        // Implement user creation logic here.
                        await userCollection.InsertOneAsync(userDetails);
                    }
                    else
                    {
                        // Handle the case where the email is not unique.
                        // Return an appropriate response indicating the duplication.
                        throw new Exception("Email is not unique.");
                    }
                }
                else
                {
                    // Handle the case where NIC is not unique.
                    // Return an appropriate response indicating the duplication.
                    throw new Exception("NIC is not unique.");
                }
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
        public async Task<UserDetails> UpdateUserAsync(string userId, UserDetails userDetails)
        {
            try
            {
                // Validate the user details using FluentValidation.
                var validator = new UserDetailsValidator();
                FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(userDetails);

                if (!validationResult.IsValid)
                {
                    // Handle validation errors and return appropriate responses.
                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }

                // Ensure that user ID is not modified during update.
                userDetails.Id = userId;

                // Retrieve the current user (assuming you have a way to identify the current user, such as from claims).
                var currentUser = GetLoggedInUser();

                if (currentUser == null)
                {
                    throw new Exception("Current user not found.");
                }

                // Set updated timestamp and updated by fields.
                userDetails.UpdatedOn = DateTime.UtcNow;
                userDetails.UpdatedBy = currentUser.Id;

                // Check if NIC (used as email) is unique before updating the user.
                if (await IsNicUniqueForUpdate(userId, userDetails.NIC))
                {
                    // Check if the email is unique before updating the user.
                    if (await IsEmailUniqueForUpdate(userId, userDetails.Email))
                    {
                        // Exclude NIC and Password fields from update.
                        var updateDefinition = Builders<UserDetails>.Update
                            .Set(u => u.FirstName, userDetails.FirstName)
                            .Set(u => u.LastName, userDetails.LastName)
                            .Set(u => u.UserType, userDetails.UserType)
                            .Set(u => u.Status, userDetails.Status)
                            .Set(u => u.IsActive, userDetails.IsActive)
                            .Set(u => u.UpdatedOn, userDetails.UpdatedOn)
                            .Set(u => u.UpdatedBy, userDetails.UpdatedBy);

                        // Implement user update logic here.
                        await userCollection.UpdateOneAsync(u => u.Id == userId, updateDefinition);

                        // Fetch and return the updated user details.
                        var updatedUser = await userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
                        return updatedUser;
                    }
                    else
                    {
                        // Handle the case where the email is not unique.
                        // Return an appropriate response indicating the duplication.
                        throw new Exception("Email is not unique.");
                    }
                }
                else
                {
                    // Handle the case where NIC is not unique.
                    // Return an appropriate response indicating the duplication.
                    throw new Exception("NIC is not unique.");
                }
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
        public async Task<string> DeleteUserAsync(string userId)
        {
            try
            {
                // Check if the user with the specified ID exists.
                var existingUser = await userCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();
                if (existingUser == null)
                {
                    // Handle the case where the user does not exist.
                    return "User not found."; // Return a custom message
                }

                // Implement user deletion logic here.
                await userCollection.DeleteOneAsync(x => x.Id == userId);

                return "User deleted successfully."; // Return a custom message upon successful deletion
            }
            catch (Exception ex)
            {
                // Handle other exceptions and return appropriate responses.
                throw ex;
            }
        }
        public async Task<UserDetails?> GetUserDetailsByIdAsync(string userId)
        {
            try
            {
                var existingUser = await userCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();
                return existingUser;
            }
            catch (Exception ex)
            {
                // Handle exceptions and return appropriate responses.
                throw ex;
            }
        }
        public async Task<(string Token, UserDetails UserDetails, bool validUser, string stage, string desc)> SignInAsync(string nic, string password)
        {
            try
            {
                // Check if a user with the given NIC exists.
                var existingUser = await userCollection.Find(x => x.NIC == nic).FirstOrDefaultAsync();
                if (existingUser == null)
                {
                    // Handle the case where the user does not exist.
                    return (null, null, false, "UserExistingValidation" ,"User Does Not Exist"); // Return null values if user not found
                }

                // Verify the password using BCrypt.
                if (!BCrypt.Net.BCrypt.Verify(password, existingUser.Password))
                {
                    // Handle the case where the password is incorrect.
                    return (null, null, false, "UserPasswordValidation", "User Password Invalid"); // Return null values if authentication failed
                }

                // Assuming existingUser is an instance of UserDetails class
                switch (existingUser.UserType)
                {
                    case UserType.Admin:
                        // For Admin, isActive should be true and status should be Default (0) to continue.
                        if (existingUser.IsActive && existingUser.Status == Status.Default)
                        {
                            // Validation passed for Admin user.
                        }
                        else if (existingUser.Status == Status.Deleted)
                        {
                            return (null, null, false, "UserStatusValidation", "User Is Deleted");
                        }
                        else if (!existingUser.IsActive)
                        {
                            return (null, null, false, "UserIsACtiveValidation", "User Is In-Active");
                        }
                        else
                        {
                            return (null, null, false, "UserStatusValidation", "User Status is not Default");
                        }
                        break;

                    case UserType.BackOffice:
                        // For BackOffice, isActive should be true and status should be Default (0) continue.
                        if (existingUser.IsActive && existingUser.Status == Status.Default)
                        {
                            // Validation passed for BackOffice user.
                        }
                        else if (existingUser.Status == Status.Deleted)
                        {
                            return (null, null, false, "UserStatusValidation", "User Is Deleted");
                        }
                        else if (!existingUser.IsActive)
                        {
                            return (null, null, false, "UserIsACtiveValidation", "User Is In-Active");
                        }
                        else
                        {
                            return (null, null, false, "UserStatusValidation", "User Status is not Default");
                        }
                        break;

                    case UserType.TravelAgent:
                        // For TravelAgent, isActive should be true and status should be Approved (2) to continue.
                        if (existingUser.IsActive && existingUser.Status == Status.Approved)
                        {
                            // Validation passed for TravelAgent user.
                        }
                        else if (existingUser.Status == Status.Deleted)
                        {
                            return (null, null, false, "UserStatusValidation", "User Is Deleted");
                        }
                        else if (!existingUser.IsActive)
                        {
                            return (null, null, false, "UserIsACtiveValidation", "User Is In-Active");
                        }
                        else
                        {
                            return (null, null, false, "UserStatusValidation", "User Status is not Approved");
                        }
                        break;

                    case UserType.User:
                        // For User, isActive should be true and status should be Default (0) || Approved (2) to continue.
                        if (existingUser.IsActive && existingUser.Status == Status.Default)
                        {
                            // Validation passed for User.
                        }
                        else if (existingUser.Status == Status.Deleted)
                        {
                            return (null, null, false, "UserStatusValidation", "User Is Deleted");
                        }
                        else if (!existingUser.IsActive)
                        {
                            return (null, null, false, "UserIsACtiveValidation", "User Is In-Active");
                        }
                        else
                        {
                            return (null, null, false, "UserStatusValidation", "User Status is not Default");
                        }
                        break;

                    default:
                        // Handle any other user types here, if needed.
                        break;
                }


                // Create claims for the JWT token.
                var claims = new[]
                {
            new Claim(ClaimTypes.NameIdentifier, existingUser.Id),
            new Claim(ClaimTypes.Name, existingUser.NIC),
            // Add other claims as needed for authorization.
        };

                // Create a security key and sign the token.
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    jwtSettings.Issuer,
                    jwtSettings.Audience,
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes),
                    signingCredentials: credentials
                );

                // Serialize the token to a string.
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return (tokenString, existingUser,  true, "", ""); // Return the token and user details upon successful sign-in
            }
            catch (Exception ex)
            {
                // Handle other exceptions and return appropriate responses.
                throw ex;
            }
        }
        public async Task SignUpAsync(UserDetails userDetails)
        {
            try
            {
                // Validate the user details using FluentValidation.
                var validator = new UserDetailsValidator();
                FluentValidation.Results.ValidationResult validationResult = validator.Validate(userDetails);

                if (!validationResult.IsValid)
                {
                    // Handle validation errors and return appropriate responses.
                    throw new FluentValidation.ValidationException(validationResult.Errors);
                }

                // Check if NIC (used as email) is unique before creating the user.
                if (await IsNicUnique(userDetails.NIC))
                {
                    // Check if the email is unique.
                    if (await IsEmailUnique(userDetails.Email))
                    {
                        // Encrypt the password before storing it.
                        userDetails.Password = BCrypt.Net.BCrypt.HashPassword(userDetails.Password); 

                        // Set the default status and isActive based on userType.
                        switch (userDetails.UserType)
                        {
                            case UserType.Admin:
                                userDetails.Status = Status.Default;
                                userDetails.IsActive = true;
                                break;
                            case UserType.BackOffice:
                                userDetails.Status = Status.Default;
                                userDetails.IsActive = true;
                                break;
                            case UserType.TravelAgent:
                                userDetails.Status = Status.New;
                                userDetails.IsActive = false;
                                break;
                            case UserType.User:
                                userDetails.Status = Status.Default;
                                userDetails.IsActive = true;
                                break;
                            default:
                                userDetails.Status = Status.Default;
                                userDetails.IsActive = true;
                                break;
                        }

                        userDetails.IsSysGenPassword = false;

                        // Implement user creation logic here.
                        await userCollection.InsertOneAsync(userDetails);
                    }
                    else
                    {
                        // Handle the case where the email is not unique.
                        // Return an appropriate response indicating the duplication.
                        throw new Exception("Email is not unique.");
                    }
                }
                else
                {
                    // Handle the case where NIC is not unique.
                    // Return an appropriate response indicating the duplication.
                    throw new Exception("NIC is not unique.");
                }
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
        private async Task<bool> IsNicUnique(string nic)
        {
            try
            {
                var existingUser = await userCollection.Find(x => x.NIC == nic).FirstOrDefaultAsync();
                return existingUser == null;
            }
            catch (Exception ex)
            {
                // Handle exceptions and return appropriate responses.
                throw ex;
            }
        }
        private async Task<bool> IsEmailUnique(string email)
        {
            try
            {
                var existingUser = await userCollection.Find(x => x.Email == email).FirstOrDefaultAsync();
                return existingUser == null;
            }
            catch (Exception ex)
            {
                // Handle exceptions and return appropriate responses.
                throw ex;
            }
        }
        private async Task<bool> IsNicUniqueForUpdate(string userId, string nic)
        {
            try
            {
                var existingUser = await userCollection.Find(x => x.Id != userId && x.NIC == nic).FirstOrDefaultAsync();
                return existingUser == null;
            }
            catch (Exception ex)
            {
                // Handle exceptions and return appropriate responses.
                throw ex;
            }
        }
        private async Task<bool> IsEmailUniqueForUpdate(string userId, string email)
        {
            try
            {
                var existingUser = await userCollection.Find(x => x.Id != userId && x.Email == email).FirstOrDefaultAsync();
                return existingUser == null;
            }
            catch (Exception ex)
            {
                // Handle exceptions and return appropriate responses.
                throw ex;
            }
        }
    }
}
