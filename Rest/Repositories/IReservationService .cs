/*
 * Filename: IReservationService.cs
 * Author: Supun Dileepa
 * Date: October 8, 2023
 * Description: This interface include all the methods that should be implelemented in ReservationService
 */


using Rest.Entities;
using Rest.Models;

namespace Rest.Repositories
{
    public interface IReservationService
    {
        Task<List<Reservation>> ReservationListAsync();
        Task<Reservation> GetReservationDetailByIdAsync(string reservationId);
        Task AddReservationAsync(Reservation reservation);
        Task UpdateReservationAsync(string reservationId, Reservation reservationDetails);
        Task DeleteReservationAsync(string reservationId);
        Task<List<Reservation>> GetReservationsByUserIdAsync(string userId);
        Task<List<Reservation>> GetReservationsByStatusAsync(string status);
    }
}
