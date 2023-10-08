/*
 * Filename: ReservationController.cs
 * Author: Supun Dileepa
 * Date: October 8, 2023
 * Description: This C# file contains the implementation of the ReservationController class, which
 *              handles HTTP requests related to reservations in the REST API.
 */

using Microsoft.AspNetCore.Mvc;
using Rest.Entities;
using Rest.Models;
using Rest.Repositories;
using static Rest.Repositories.ScheduleService;

namespace Rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService reservationService;
        private readonly IScheduleService scheduleService;

        public ReservationController(IReservationService reservationService, IScheduleService scheduleService)
        {
            this.reservationService = reservationService;
            this.scheduleService = scheduleService;
        }
        // Get all Reservations
        [HttpGet]
        public async Task<List<Reservation>> Get()
        {
            return await reservationService.ReservationListAsync();
        }

        // Get one Reservation By ID
        [HttpGet("{reservationId:length(24)}")]
        public async Task<ActionResult<Reservation>> Get(string reservationId)
        {
            var reservationDetails = await reservationService.GetReservationDetailByIdAsync(reservationId);
            if (reservationDetails is null)
            {
                return NotFound();
            }
            return reservationDetails;
        }

        // Create new Reservation
        [HttpPost]
        public async Task<IActionResult> Post(Reservation reservationDetails)
        {
            await reservationService.AddReservationAsync(reservationDetails);
            return CreatedAtAction(nameof(Get), new { id = reservationDetails.Id }, reservationDetails);
        }

        // Update existing Reservation by ID
        [HttpPut("{reservationId:length(24)}")]
        public async Task<IActionResult> Update(string reservationId, Reservation reservationDetails)
        {
            var existingReservation = await reservationService.GetReservationDetailByIdAsync(reservationId);
            if (existingReservation is null)
            {
                return NotFound();
            }
            reservationDetails.Id = reservationId;
            await reservationService.UpdateReservationAsync(reservationId, reservationDetails);
            return Ok();
        }

        // Delete Reservation by ID
        [HttpDelete("{reservationId:length(24)}")]
        public async Task<IActionResult> Delete(string reservationId)
        {
            var existingReservation = await reservationService.GetReservationDetailByIdAsync(reservationId);
            if (existingReservation is null)
            {
                return NotFound();
            }
            await reservationService.DeleteReservationAsync(reservationId);
            return Ok();
        }

        // Create new Reservation for Existing Schedule
        [HttpPost("createForSchedule/{scheduleId:length(24)}")]
        public async Task<IActionResult> CreateReservationForSchedule(string scheduleId, Reservation reservation)
        {
            try
            {
                // Check if the schedule with the given ID exists
                var existingSchedule = await scheduleService.GetScheduleDetailByIdAsync(scheduleId);
                if (existingSchedule is null)
                {
                    return NotFound("Schedule not found.");
                }


                // Create the reservation
                await scheduleService.AddReservationToScheduleAsync(scheduleId, reservation);

                return CreatedAtAction(nameof(Get), new { id = reservation.Id }, reservation);
            }
            catch (ScheduleNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ReservedCountExceedsTotalSeatsException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        // Update Reservation in existing schedule
        [HttpPut("updateReservationForSchedule/{scheduleId:length(24)}/{reservationId:length(24)}")]
        public async Task<IActionResult> UpdateReservationForSchedule(string scheduleId, string reservationId, Reservation updatedReservation)
        {
            try
            {
                // Check if the schedule with the given ID exists
                var existingSchedule = await scheduleService.GetScheduleDetailByIdAsync(scheduleId);
                if (existingSchedule is null)
                {
                    return NotFound("Schedule not found.");
                }

                // Find the reservation to be updated
                var reservationToUpdate = existingSchedule.reservations?.FirstOrDefault(r => r.Id == reservationId);
                if (reservationToUpdate == null)
                {
                    return NotFound("Reservation not found.");
                }

                // Calculate the total reserved seats excluding the reservation to be updated
                var totalSeats = existingSchedule.train?.TotalSeats ?? 0;
                var reservedCount = existingSchedule.reservations?
                    .Where(r => r.Id != reservationId && r.ReservationStatus == "RESERVED")
                    .Sum(r => r.ReservedCount) ?? 0;

                if (reservedCount + updatedReservation.ReservedCount > totalSeats)
                {
                    return BadRequest("ReservedCount exceeds TotalSeats");
                }

                // Update the reservation with the new values
                reservationToUpdate.ReservedCount = updatedReservation.ReservedCount;
                reservationToUpdate.DisplayName = updatedReservation.DisplayName;
                reservationToUpdate.ReservationDate = updatedReservation.ReservationDate;
                reservationToUpdate.ReservationStatus = updatedReservation.ReservationStatus;
                reservationToUpdate.Amount = updatedReservation.Amount;

                // Save the changes to the database
             
                await scheduleService.UpdateScheduleAsync(existingSchedule);

                var existingReservation = await reservationService.GetReservationDetailByIdAsync(reservationId);
                if (existingReservation is null)
                {
                    return NotFound();
                }
                updatedReservation.Id = reservationId;
                await reservationService.UpdateReservationAsync(reservationId, updatedReservation);


                return Ok("Reservation updated successfully");
            }
            catch (ScheduleNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                return StatusCode(500, "An error occurred while processing the request.");
            }

        }



        // Get shcedules reserved by user ID
        [HttpGet("getSchedulesByUserId/{userId}")]
        public async Task<IActionResult> GetSchedulesByUserId(string userId)
        {
            try
            {
                // Find all reservations for the given user ID
                var reservations = await reservationService.GetReservationsByUserIdAsync(userId);

                // Extract schedule IDs from the reservations
                var scheduleIds = reservations.Select(r => r.ScheduleId).Distinct().ToList();

                // Retrieve schedules using the extracted schedule IDs
                var schedules = new List<Schedule>();

                foreach (var scheduleId in scheduleIds)
                {
                    var schedule = await scheduleService.GetScheduleDetailByIdAsync(scheduleId);
                    if (schedule != null)
                    {
                        schedules.Add(schedule);
                    }
                }

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        // Get Reservations by User ID
        [HttpGet("getReservationsByUserId/{userId}")]
        public async Task<IActionResult> GetReservationsByUserId(string userId)
        {
            try
            {
                // Call your reservation service to get reservations by user ID
                var reservations = await reservationService.GetReservationsByUserIdAsync(userId);

                if (reservations == null || reservations.Count == 0)
                {
                    return NotFound("No reservations found for the user.");
                }

                return Ok(reservations);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        // Get Reservations By Status
        [HttpGet("getReservationsByStatus/{status}")]
        public async Task<IActionResult> GetReservationsByStatus(string status)
        {
            try
            {
                // Call your reservation service to get reservations by status
                var reservations = await reservationService.GetReservationsByStatusAsync(status);

                if (reservations == null || reservations.Count == 0)
                {
                    return NotFound("No reservations found with the specified status.");
                }

                return Ok(reservations);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
    }
}