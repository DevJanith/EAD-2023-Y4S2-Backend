using Microsoft.AspNetCore.Mvc;
using Rest.Entities;
using Rest.Repositories;

namespace Rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRequestController : ControllerBase
    {
        private readonly IUserRequestService userRequestService;

        public UserRequestController(IUserRequestService userRequestService)
        {
            this.userRequestService = userRequestService;
        }

        // GET: api/UserRequest
        [HttpGet]
        public async Task<IActionResult> GetUserRequestsAsync(int page, int perPage, string direction)
        {
            try
            { 

                var (userRequests, total) = await userRequestService.GetUserRequestsAsync(page, perPage, direction);
                var result = new
                {
                    UserRequests = userRequests,
                    Total = total
                };
                return Ok(result); // 200 OK
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = "Invalid argument", Message = ex.Message }); // 400 Bad Request
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message }); // 500 Internal Server Error
            }
        }

        // POST: api/UserRequest
        [HttpPost]
        public async Task<IActionResult> CreateUserRequestAsync(UserRequest userRequest)
        {
            try
            {
                var (validUser, stage, desc) = await userRequestService.CreateUserRequestAsync(userRequest);
                if (!validUser)
                {
                    switch (stage)
                    {
                        case "UserExistingValidation":
                            return Unauthorized(new { Error = "Authentication failed", Message = desc }); // 401 Unauthorized  

                        case "UserIsACtiveValidation":
                            return Unauthorized(new { Error = "Authentication failed", Message = desc }); // 401 Unauthorized 

                        case "UserStatusValidation":
                            return Unauthorized(new { Error = "Authentication failed", Message = desc }); // 401 Unauthorized 

                        default:
                            return Unauthorized(new { Error = "Authentication failed", Message = "Invalid user" }); // 401 Unauthorized 
                    }
                }
                return Ok(new { Message = "User request created successfully" }); // 200 OK
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { Error = "Validation error", Message = ex.Errors }); // 400 Bad Request
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message }); // 500 Internal Server Error
            }
        }

        // PUT: api/UserRequest/{id}
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> UpdateUserRequest(string id, UserRequest userRequest)
        {
            try
            {
                var updatedUserRequest = await userRequestService.UpdateUserRequestAsync(id, userRequest);
                return Ok(updatedUserRequest);// Return the updated user data with a 200 OK status code
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { Error = "Validation error", Message = ex.Errors }); // 400 Bad Request
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message }); // 500 Internal Server Error
            }
        }

        // DELETE: api/UserRequest/{id}
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeleteUserRequest(string id)
        {
            try
            {
                var deleteMessage = await userRequestService.DeleteUserRequestAsync(id);
                if (deleteMessage == "User request not found.")
                {
                    return NotFound(new { Error = "User not found", Message = "The user request to be deleted was not found." }); // 404 Not Found
                }

                return Ok(new { Message = deleteMessage }); // 200 OK with a custom message
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message }); // 500 Internal Server Error
            }
        }
    }
}
