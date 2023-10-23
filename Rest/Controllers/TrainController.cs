/*
 * Filename: TrainController.cs
 * Author: Ridma Dilshan
 * ID Number : IT20005276
 * Date: October 8, 2023
 * Description: This C# file contains the implementation of the TrainController class, which
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
    public class TrainController : ControllerBase
    {
        private readonly ITrainService trainService;
        public TrainController(ITrainService trainService) => this.trainService = trainService;

        // Get All Trains
        [HttpGet]
        public async Task<List<Train>> Get()
        {
            return await trainService.TrainListAsync();
        }

        // Get one Train
        [HttpGet("{trainId:length(24)}")]
        public async Task<ActionResult<Train>> Get(string trainId)
        {
            var trainDetails = await trainService.GetTrainDetailByIdAsync(trainId);
            if (trainDetails is null)
            {
                return NotFound();
            }
            return trainDetails;
        }

        // Create new Train
        [HttpPost]
        public async Task<IActionResult> Post(Train trainDetails)
        {
            await trainService.AddTrainAsync(trainDetails);
            return CreatedAtAction(nameof(Get), new
            {
                id = trainDetails.Id,
            }, trainDetails);
        }

        // Update existing Train
        [HttpPut("{trainId:length(24)}")]
        public async Task<IActionResult> Update(string trainId, Train trainDetails)
        {
            var trainDetail = await trainService.GetTrainDetailByIdAsync(trainId);
            if (trainDetail is null)
            {
                return NotFound();
            }
            trainDetail.Id = trainDetail.Id;
            await trainService.UpdateTrainAsync(trainId, trainDetails);
            return Ok();
        }

        // Delete Train By ID
        [HttpDelete("{trainId:length(24)}")]
        public async Task<IActionResult> Delete(string trainId)
        {
            var trainDetail = await trainService.GetTrainDetailByIdAsync(trainId);
            if (trainDetail is null)
            {
                return NotFound();
            }
            await trainService.DeleteTrainAsync(trainId);
            return Ok();
        }
    }
}
