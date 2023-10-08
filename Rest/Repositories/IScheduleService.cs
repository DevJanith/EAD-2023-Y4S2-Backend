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
        Task AddReservationToScheduleAsync(string scheduleId, Reservation reservation);
        Task UpdateScheduleAsync(Schedule schedule);
        Task<List<Schedule>> GetSchedulesByStatusAsync(string status);
        Task<List<Schedule>> GetIncomingSchedules();

    }

}
