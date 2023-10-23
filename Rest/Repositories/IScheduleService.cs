/*
* Filename: IScheduleService.cs
* Author: Himasha Ranaweera
* ID Number : IT20251000
* Date: October 8, 2023
* Description: This interface include all the methods that should be implelemented in ScheduleService
*/



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
