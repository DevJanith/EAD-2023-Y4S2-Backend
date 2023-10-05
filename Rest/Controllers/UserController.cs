using Microsoft.AspNetCore.Mvc;
using Rest.Entities;
using Rest.Repositories;
using System;
using System.Threading.Tasks;

namespace Rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        // GET: api/User
        [HttpGet]
        public async Task<IActionResult> GetUsersAsync(int page, int perPage, string direction, Status? status, UserType? userType, bool? isActive)
        {
            try
            {
                var (users, total) = await userService.GetUsersAsync(page, perPage, direction, status, userType, isActive);
                var result = new
                {
                    Users = users,
                    Total = total
                };
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid argument: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/User
        [HttpPost]
        public async Task<IActionResult> CreateUserAsync(UserDetails userDetails)
        {
            try
            {
                await userService.CreateUserAsync(userDetails);
                return CreatedAtAction(nameof(GetUserById), new { userId = userDetails.Id }, userDetails);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/User/{userId}
        [HttpGet("{userId:length(24)}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            try
            {
                var userDetails = await userService.GetUserDetailsByIdAsync(userId);
                if (userDetails == null)
                {
                    return NotFound();
                }
                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/User/{userId}
        [HttpPut("{userId:length(24)}")]
        public async Task<IActionResult> UpdateUser(string userId, UserDetails userDetails)
        {
            try
            {
                await userService.UpdateUserAsync(userId, userDetails);
                return Ok();
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/User/{userId}
        [HttpDelete("{userId:length(24)}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                await userService.DeleteUserAsync(userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/User/SignIn
        [HttpPost("SignIn")]
        public async Task<IActionResult> SignInAsync(string nic, string password)
        {
            try
            {
                var token = await userService.SignInAsync(nic, password);
                if (token == null)
                {
                    return Unauthorized("Authentication failed.");
                }
                return Ok(token);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/User/SignUp
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUpAsync(UserDetails userDetails)
        {
            try
            {
                await userService.SignUpAsync(userDetails);
                return CreatedAtAction(nameof(GetUserById), new { userId = userDetails.Id }, userDetails);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/User/GetCurrentUser
        [HttpGet("GetCurrentUser")]
        public IActionResult GetLoggedInUser()
        {
            try
            {
                var loggedInUser = userService.GetLoggedInUser();
                if (loggedInUser == null)
                {
                    return Unauthorized("User not authenticated or not found.");
                }
                return Ok(loggedInUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
