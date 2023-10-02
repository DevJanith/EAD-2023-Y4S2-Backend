using Rest.Entities;

namespace Rest.Repositories
{
    public interface IUserService
    {
        Task<List<UserDetails>> GetUserListAsync();
        Task<UserDetails> GetUserDetailsByIdAsync(string userId);
        Task CreateUserAsync(UserDetails userDetails);
        Task UpdateUserAsync(string userId, UserDetails userDetails);
        Task DeleteUserAsync(string userId);
        Task ActivateUserAsync(string userId);
        Task RequestDeactivationAsync(string userId);
    }
}
