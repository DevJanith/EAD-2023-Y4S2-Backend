using Microsoft.AspNetCore.Mvc;
using Rest.Entities;
using Rest.Models;
using Rest.Repositories;

namespace Rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService reservationService;

        public ReservationController(IReservationService reservationService)
        {
            this.reservationService = reservationService;
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
    }
}