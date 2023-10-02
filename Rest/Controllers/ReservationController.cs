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

        [HttpGet]
        public async Task<List<Reservation>> Get()
        {
            return await reservationService.ReservationListAsync();
        }

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

        [HttpPost]
        public async Task<IActionResult> Post(Reservation reservationDetails)
        {
            await reservationService.AddReservationAsync(reservationDetails);
            return CreatedAtAction(nameof(Get), new { id = reservationDetails.Id }, reservationDetails);
        }

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

                // Perform any validation checks on the reservation object if needed

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
                    .Where(r => r.Id != reservationId)
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
    }
}