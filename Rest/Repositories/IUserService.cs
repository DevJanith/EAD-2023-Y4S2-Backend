/*
 * Filename: IUserService.cs
 * Author: Janith Gamage
 * Date: October 8, 2023
 * Description: This interface include all the methods that should be implelemented in User Details service
 */

using Rest.Entities;

namespace Rest.Repositories
{
    public interface IUserService
    {
        Task<(List<UserDetails> Users, int Total)> GetUsersAsync(int page, int perPage, string direction, Status? status, List<UserType> userTypes, bool? isActive);
        Task CreateUserAsync(UserDetails userDetails);
        Task<UserDetails> UpdateUserAsync(string userId, UserDetails userDetails);
        Task<string> DeleteUserAsync(string userId);
        Task<UserDetails?> GetUserDetailsByIdAsync(string userId);
        Task<(string Token, UserDetails UserDetails,bool validUser, string stage, string desc)> SignInAsync(string nic, string password);
        Task SignUpAsync(UserDetails userDetails);
        UserDetails? GetLoggedInUser();
    }
}
