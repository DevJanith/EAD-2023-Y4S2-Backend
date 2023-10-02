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
