using Microsoft.AspNetCore.Mvc;
using Rest.Entities;
using Rest.Repositories;

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

        [HttpGet]
        public async Task<List<UserDetails>> Get()
        {
            return await userService.GetUserListAsync();
        }

        [HttpGet("{userId:length(24)}")]
        public async Task<ActionResult<UserDetails>> Get(string userId)
        {
            var userDetails = await userService.GetUserDetailsByIdAsync(userId);
            if (userDetails is null)
            {
                return NotFound();
            }
            return userDetails;
        }

        [HttpPost]
        public async Task<IActionResult> Post(UserDetails userDetails)
        {
            await userService.CreateUserAsync(userDetails);
            return CreatedAtAction(nameof(Get), new { id = userDetails.Id }, userDetails);
        }

        [HttpPut("{userId:length(24)}")]
        public async Task<IActionResult> Update(string userId, UserDetails userDetails)
        {
            var existingUserDetails = await userService.GetUserDetailsByIdAsync(userId);
            if (existingUserDetails is null)
            {
                return NotFound();
            }
            userDetails.Id = userId; // Ensure that user ID is not modified during update.
            await userService.UpdateUserAsync(userId, userDetails);
            return Ok();
        }

        [HttpDelete("{userId:length(24)}")]
        public async Task<IActionResult> Delete(string userId)
        {
            var userDetails = await userService.GetUserDetailsByIdAsync(userId);
            if (userDetails is null)
            {
                return NotFound();
            }
            await userService.DeleteUserAsync(userId);
            return Ok();
        }

        [HttpPost("{userId:length(24)}/activate")]
        public async Task<IActionResult> Activate(string userId)
        {
            await userService.ActivateUserAsync(userId);
            return Ok();
        }

        [HttpPost("{userId:length(24)}/request-deactivation")]
        public async Task<IActionResult> RequestDeactivation(string userId)
        {
            await userService.RequestDeactivationAsync(userId);
            return Ok();
        }
    }
}
