namespace NovaStayHotel;
public interface IReservationService
{
    Task<ReservationDetail> GetReservationAsync(long reservationId);
    Task<IEnumerable<ReservationDetail>> GetReservationsAsync(ReservationCriteria criteria);
    Task<long> AddReservationAsync(ReservationDetail reservation);
    Task UpdateReservationAsync(ReservationDetail reservation);
    Task DeleteReservationAsync(long reservationId);
}

