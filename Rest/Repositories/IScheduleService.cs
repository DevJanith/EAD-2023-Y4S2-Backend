using Rest.Entities;
using Rest.Models;

namespace Rest.Repositories
{
    public interface IScheduleService
    {
        Task<List<Schedule>> ScheduleListAsync();
        Task<Schedule> GetScheduleDetailByIdAsync(string scheduleId);
        Task AddScheduleAsync(Schedule schedule);
        Task UpdateScheduleAsync(string scheduleId, Schedule scheduleDetails);
        Task DeleteScheduleAsync(string scheduleId);
    }

}
