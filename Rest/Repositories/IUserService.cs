using Rest.Entities;

namespace Rest.Repositories
{
    public interface IUserService
    { 
        Task<(List<UserDetails> Users, int Total)> GetUsersAsync(int page, int perPage, string direction, Status? status, UserType? userType, bool? isActive);
        Task CreateUserAsync(UserDetails userDetails);
        Task<UserDetails> UpdateUserAsync(string userId, UserDetails userDetails);
        Task<string> DeleteUserAsync(string userId);
        Task<UserDetails?> GetUserDetailsByIdAsync(string userId);
        Task<(string Token, UserDetails UserDetails)> SignInAsync(string nic, string password);
        Task SignUpAsync(UserDetails userDetails);
        UserDetails? GetLoggedInUser();
    }
}
