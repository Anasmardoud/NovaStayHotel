namespace NovaStayHotel;
public interface IGuestService
{
    Task<GuestDetail> GetGuestAsync(long id);
    Task<IEnumerable<GuestDetail>> GetGuestsAsync(GuestCriteria criteria);
    Task<long> AddGuestAsync(GuestDetail guest);
    Task UpdateGuestAsync(GuestDetail guest);
    Task DeleteGuestAsync(long id);
}
