namespace NovaStayHotel;

public interface IRoomService
{
    Task<RoomDetail> GetRoomAsync(long roomId);
    Task<IEnumerable<RoomDetail>> GetRoomsAsync(RoomCriteria criteria);
    Task<long> AddRoomAsync(RoomDetail room);
    Task UpdateRoomAsync(RoomDetail room);
    Task DeleteRoomAsync(long roomId);
}
