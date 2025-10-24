using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Helpers.Errors.Model;
using System.Transactions;

namespace NovaStayHotel;
public class RoomService : IRoomService
{
    #region Construction
    readonly NovaStayHotelDataContext Context;
    public RoomService(NovaStayHotelDataContext context) => Context = context;
    #endregion

    #region Get Rooms
    public async Task<RoomDetail> GetRoomAsync(long id)
    {
        return await (from x in Context.Rooms
                      where x.Id == id
                      select ProjectRoom(x)).FirstOrDefaultAsync() ?? throw new NotFoundException();
    }

    public async Task<IEnumerable<RoomDetail>> GetRoomsAsync(RoomCriteria criteria)
    {
        criteria.Fixup();
        return await (from x in Context.Rooms
                      where (criteria.MinRoomNumber == null || x.Number >= criteria.MinRoomNumber)
                         && (criteria.MaxRoomNumber == null || x.Number <= criteria.MaxRoomNumber)
                         && (criteria.MinFloorNumber == null || x.FloorNumber >= criteria.MinFloorNumber)
                         && (criteria.MaxFloorNumber == null || x.FloorNumber <= criteria.MaxFloorNumber)
                         && (criteria.Types.IsNullOrEmpty() || criteria.Types.Contains(x.Type))
                         && (criteria.Statuses.IsNullOrEmpty() || criteria.Statuses.Contains(x.Status))
                         && (criteria.HasBalcony == null || x.HasBalcony == criteria.HasBalcony)
                         && (criteria.MinPricePerNightUsd == null || x.PricePerNightUsd >= criteria.MinPricePerNightUsd)
                         && (criteria.MaxPricePerNightUsd == null || x.PricePerNightUsd <= criteria.MaxPricePerNightUsd)
                      orderby x.Id descending
                      select ProjectRoom(x)).ToListAsync();
    }
    #endregion

    #region Add Room
    public async Task<long> AddRoomAsync(RoomDetail source)
    {
        source.Fixup();
        source.Validate();
        bool roomExists = await Context.Rooms.AnyAsync(x => x.Id == source.Id || x.Number == source.Number);
        if (roomExists)
            throw new Exception("Room already exists");
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var target = new Room();
        await Context.AddAsync(target);
        UpsertRoom(target, source);
        target.CreatedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        transaction.Complete();
        return target.Id;
    }
    #endregion

    #region Update Room
    public async Task UpdateRoomAsync(RoomDetail source)
    {
        source.Fixup();
        source.Validate();
        var target = await Context.Rooms.FindAsync(source.Id) ?? throw new NotFoundException();
        var original = ProjectRoom(target);
        var duplicateRoomExists = await Context.Rooms.AnyAsync(x => x.Id != source.Id && x.Number == source.Number);
        if (duplicateRoomExists)
            throw new Exception("Room already exists");
        UpsertRoom(target, source, original);
        await Context.SaveChangesAsync();
    }
    #endregion

    #region Delete Room
    public async Task DeleteRoomAsync(long id)
    {
        var target = await Context.Rooms.FindAsync(id) ?? throw new NotFoundException();
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        Context.Rooms.Remove(target);
        await Context.SaveChangesAsync();
        transaction.Complete();
    }
    #endregion

    #region Projection
    internal static RoomDetail ProjectRoom(Room room)
    {
        return new RoomDetail
        {
            Id = room.Id,
            Number = room.Number,
            FloorNumber = room.FloorNumber,
            Type = room.Type,
            Status = room.Status,
            HasBalcony = room.HasBalcony,
            PricePerNightUsd = room.PricePerNightUsd,
            LastMaintainedAt = room.LastMaintainedAt,
            Description = room.Description
        };
    }
    #endregion

    #region Upsert
    static void UpsertRoom(Room target, RoomDetail source, RoomDetail? original = null)
    {
        target.Number = source.Number;
        target.FloorNumber = source.FloorNumber;
        target.Type = source.Type;
        target.Status = source.Status;
        target.HasBalcony = source.HasBalcony;
        target.PricePerNightUsd = source.PricePerNightUsd;
        target.LastMaintainedAt = source.LastMaintainedAt;
        target.Description = source.Description;
        if (original != source)
            target.UpdatedAt = DateTime.UtcNow;
    }
    #endregion
}
