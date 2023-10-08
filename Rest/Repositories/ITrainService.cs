/*
 * Filename: ITrainService.cs
 * Author: Supun Dileepa
 * Date: October 8, 2023
 * Description: This interface include all the methods that should be implelemented in TrainService
 */


using Rest.Entities;
using Rest.Models;

namespace Rest.Repositories
{
    public interface ITrainService
    {
        public Task<List<Train>> TrainListAsync();
        public Task<Train> GetTrainDetailByIdAsync(string trainId);
        public Task AddTrainAsync(Train train);
        public Task UpdateTrainAsync(string trainId, Train trainDetails);
        public Task DeleteTrainAsync(string trainId);
    }
}
