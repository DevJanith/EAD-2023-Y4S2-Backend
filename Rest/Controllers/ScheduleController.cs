using Microsoft.AspNetCore.Mvc;
using Rest.Entities;
using Rest.Models;
using Rest.Repositories;

namespace Rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            this.scheduleService = scheduleService;
        }

        [HttpGet]
        public async Task<List<Schedule>> Get()
        {
            return await scheduleService.ScheduleListAsync();
        }

        [HttpGet("{scheduleId:length(24)}")]
        public async Task<ActionResult<Schedule>> Get(string scheduleId)
        {
            var scheduleDetails = await scheduleService.GetScheduleDetailByIdAsync(scheduleId);
            if (scheduleDetails is null)
            {
                return NotFound();
            }
            return scheduleDetails;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Schedule scheduleDetails)
        {
            await scheduleService.AddScheduleAsync(scheduleDetails);
            return CreatedAtAction(nameof(Get), new { id = scheduleDetails.Id }, scheduleDetails);
        }

        [HttpPut("{scheduleId:length(24)}")]
        public async Task<IActionResult> Update(string scheduleId, Schedule scheduleDetails)
        {
            var existingSchedule = await scheduleService.GetScheduleDetailByIdAsync(scheduleId);
            if (existingSchedule is null)
            {
                return NotFound();
            }
            scheduleDetails.Id = scheduleId;
            await scheduleService.UpdateScheduleAsync(scheduleId, scheduleDetails);
            return Ok();
        }

        [HttpDelete("{scheduleId:length(24)}")]
        public async Task<IActionResult> Delete(string scheduleId)
        {
            var existingSchedule = await scheduleService.GetScheduleDetailByIdAsync(scheduleId);
            if (existingSchedule is null)
            {
                return NotFound();
            }
            await scheduleService.DeleteScheduleAsync(scheduleId);
            return Ok();
        }
    }
}