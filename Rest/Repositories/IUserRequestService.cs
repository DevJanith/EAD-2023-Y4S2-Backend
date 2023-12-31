﻿using Rest.Entities;

namespace Rest.Repositories
{
    public interface IUserRequestService
    {
        Task<(List<UserRequest> UserRequests, int Total)> GetUserRequestsAsync(int page, int perPage, string direction);
        Task<(bool validUser, string stage, string desc)> CreateUserRequestAsync(UserRequest userRequest);
        Task<UserRequest> UpdateUserRequestAsync(string id, UserRequest userRequest);
        Task<string> DeleteUserRequestAsync(string Id);
    }
}
