/*
 * Filename: UserController.cs
 * Author: Janith Gamage
 * ID Number : IT20402266
 * Date: October 8, 2023
 * Description: This C# file contains the implementation of the UserController class, which
 *              handles HTTP user related functions.
 */

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
        public async Task<IActionResult> GetUsersAsync(int page, int perPage, string direction, Status? status, string userTypes, bool? isActive)
        {
            try
            {
                List<UserType> userTypeList = ParseUserTypes(userTypes);

                var (users, total) = await userService.GetUsersAsync(page, perPage, direction, status, userTypeList, isActive);
                var result = new
                {
                    Users = users,
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

        // POST: api/User
        [HttpPost]
        public async Task<IActionResult> CreateUserAsync(UserDetails userDetails)
        {
            try
            {
                await userService.CreateUserAsync(userDetails);
                return CreatedAtAction(nameof(GetUserById), new { userId = userDetails.Id }, userDetails); // 201 Created
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

        // GET: api/User/{userId}
        [HttpGet("{userId:length(24)}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            try
            {
                var userDetails = await userService.GetUserDetailsByIdAsync(userId);
                if (userDetails == null)
                {
                    return NotFound(); // 404 Not Found
                }
                return Ok(userDetails); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message }); // 500 Internal Server Error
            }
        }

        // PUT: api/User/{userId}
        [HttpPut("{userId:length(24)}")]
        public async Task<IActionResult> UpdateUser(string userId, UserDetails userDetails)
        {
            try
            {
                var updatedUser = await userService.UpdateUserAsync(userId, userDetails);
                return Ok(updatedUser);// Return the updated user data with a 200 OK status code
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

        // DELETE: api/User/{userId}
        [HttpDelete("{userId:length(24)}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var deleteMessage = await userService.DeleteUserAsync(userId);
                if (deleteMessage == "User not found.")
                {
                    return NotFound(new { Error = "User not found", Message = "The user to be deleted was not found." }); // 404 Not Found
                }

                return Ok(new { Message = deleteMessage }); // 200 OK with a custom message
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message }); // 500 Internal Server Error
            }
        }

        // POST: api/User/SignIn
        [HttpPost("SignIn")]
        public async Task<IActionResult> SignInAsync(SignInModel signInModel)
        {
            try
            {
                var (token, userDetails, validUser, stage, desc) = await userService.SignInAsync(signInModel.NIC, signInModel.Password);
                if (!validUser)
                {
                    switch (stage)
                    {
                        case "UserExistingValidation":
                            return Unauthorized(new { Error = "Authentication failed", Message = desc }); // 401 Unauthorized 

                        case "UserPasswordValidation":
                            return Unauthorized(new { Error = "Authentication failed", Message = desc }); // 401 Unauthorized 

                        case "UserIsACtiveValidation":
                            return Unauthorized(new { Error = "Authentication failed", Message = desc }); // 401 Unauthorized 

                        case "UserStatusValidation":
                            return Unauthorized(new { Error = "Authentication failed", Message = desc }); // 401 Unauthorized 

                        default:
                            return Unauthorized(new { Error = "Authentication failed", Message = "Invalid user" }); // 401 Unauthorized 
                    } 
                }

                return Ok(new { Token = token, UserDetails = userDetails }); // 200 OK with token and user details
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message }); // 500 Internal Server Error
            }
        }

        // POST: api/User/SignUp
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUpAsync(UserDetails userDetails)
        {
            try
            {
                await userService.SignUpAsync(userDetails);
                return CreatedAtAction(nameof(GetUserById), new { userId = userDetails.Id }, userDetails); // 201 Created
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

        // GET: api/User/GetCurrentUser
        [HttpGet("GetCurrentUser")]
        public IActionResult GetLoggedInUser()
        {
            try
            {
                var loggedInUser = userService.GetLoggedInUser();
                if (loggedInUser == null)
                {
                    return Unauthorized(new { Error = "Authentication failed", Message = "User not authenticated or not found" }); // 401 Unauthorized
                }
                return Ok(loggedInUser); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal server error", Message = ex.Message }); // 500 Internal Server Error
            }
        }

        public class SignInModel
        {
            public string NIC { get; set; }
            public string Password { get; set; }
        }
        private List<UserType> ParseUserTypes(string userTypes)
        {
            if (string.IsNullOrEmpty(userTypes))
            {
                return Enum.GetValues(typeof(UserType)).Cast<UserType>().ToList();
            }

            var userTypeList = new List<UserType>();
            var userTypeStrings = userTypes.Split(',').Select(s => s.Trim());

            foreach (var userTypeString in userTypeStrings)
            {
                if (Enum.TryParse(userTypeString, true, out UserType userType))
                {
                    userTypeList.Add(userType);
                }
            }

            return userTypeList;
        }

    }
}
