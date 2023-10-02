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
