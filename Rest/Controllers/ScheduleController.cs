/*
 * Filename: ScheduleController.cs
 * Author: Supun Dileepa
 * Date: October 8, 2023
 * Description: This C# file contains the implementation of the ScheduleController class, which
 *              handles HTTP requests related to reservations in the REST API.
 */


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
        private readonly ITrainService trainService;

        public ScheduleController(IScheduleService scheduleService, ITrainService trainService)
        {
            this.scheduleService = scheduleService;
            this.trainService = trainService;
        }
        // Get all Schedules
        [HttpGet]
        public async Task<List<Schedule>> Get()
        {
            return await scheduleService.ScheduleListAsync();
        }
        
        // Get all Schedule by ID
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
       
        // Create new Schedule
        [HttpPost]
        public async Task<IActionResult> Post(Schedule scheduleDetails)
        {
            await scheduleService.AddScheduleAsync(scheduleDetails);
            return CreatedAtAction(nameof(Get), new { id = scheduleDetails.Id }, scheduleDetails);
        }

        // Update existing Schedule
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

        // Delete Schedule by ID
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

        // Get Incoming Schedules by Status
        [HttpGet("getSchedulesByStatus/{status}")]
        public async Task<IActionResult> GetSchedulesByStatus(string status)
        {
            try
            {
                // Call your schedule service to get schedules by status
                var schedules = await scheduleService.GetSchedulesByStatusAsync(status);

                if (schedules == null || schedules.Count == 0)
                {
                    return NotFound("No schedules found with the specified status.");
                }

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        // Get all incoming Schedules
        [HttpGet("getIncomingSchedules")]
        public async Task<IActionResult> GetIncomingSchedules()
        {
            try
            {
                // Call your schedule service to get schedules by status
                var schedules = await scheduleService.GetIncomingSchedules();

                if (schedules == null || schedules.Count == 0)
                {
                    return NotFound("No schedules found.");
                }

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        // Add new Tarin to Schedule
        [HttpPost("addTrainToSchedule/{scheduleId:length(24)}/{trainId:length(24)}")]
        public async Task<IActionResult> AddTrainToSchedule(string scheduleId, string trainId)
        {
            try
            {
                // Check if the schedule with the given ID exists
                var existingSchedule = await scheduleService.GetScheduleDetailByIdAsync(scheduleId);
                if (existingSchedule == null)
                {
                    return NotFound("Schedule not found.");
                }

                // Check if the train with the given ID exists
                var existingTrain = await trainService.GetTrainDetailByIdAsync(trainId);
                if (existingTrain == null)
                {
                    return NotFound("Train not found.");
                }

                // Check if the train is active and published
                if (existingTrain.Status != "ACTIVE" || existingTrain.PublishStatus != "PUBLISHED")
                {
                    return BadRequest("The train must be ACTIVE and PUBLISHED to be added to the schedule.");
                }

                existingSchedule.train = existingTrain;

                // Update the schedule in the database
                await scheduleService.UpdateScheduleAsync(scheduleId, existingSchedule);

                return Ok("Train added to schedule successfully.");
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }



    }
}