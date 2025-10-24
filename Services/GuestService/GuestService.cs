using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using System.Transactions;

namespace NovaStayHotel;
public class GuestService : IGuestService
{
    #region Construction
    readonly NovaStayHotelDataContext Context;
    public GuestService(NovaStayHotelDataContext context) => Context = context;
    #endregion

    #region Get Guests
    public async Task<GuestDetail> GetGuestAsync(long id)
    {
        return await (from x in Context.Guests
                      where x.Id == id
                      select ProjectGuest(x)).FirstOrDefaultAsync() ?? throw new NotFoundException();
    }

    public async Task<IEnumerable<GuestDetail>> GetGuestsAsync(GuestCriteria criteria)
    {
        criteria.Fixup();
        return await (from x in Context.Guests
                      let n = x.FirstName + " " + x.MiddleName + " " + x.LastName
                      where (criteria.Name == null || n.StartsWith(criteria.Name, true, null))
                         && (criteria.PhoneNumber == null || x.PhoneNumber.StartsWith(criteria.PhoneNumber, true, null))
                      orderby x.Id descending
                      select ProjectGuest(x)).ToListAsync();
    }
    #endregion

    #region Add Guest
    public async Task<long> AddGuestAsync(GuestDetail source)
    {
        source.Fixup();
        source.Validate();
        bool guestExists = await Context.Guests.AnyAsync(x => x.Id == source.Id);
        if (guestExists)
            throw new ArgumentException("Guest already exists");
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var target = new Guest();
        await Context.Guests.AddAsync(target);
        UpsertGuest(target, source);
        target.CreatedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        transaction.Complete();
        return target.Id;
    }
    #endregion

    #region Update Guest
    public async Task UpdateGuestAsync(GuestDetail source)
    {
        source.Fixup();
        source.Validate();
        var target = await Context.Guests.FirstOrDefaultAsync(x => x.Id == source.Id) ?? throw new NotFoundException();
        var original = ProjectGuest(target);
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        UpsertGuest(target, source, original);
        await Context.SaveChangesAsync();
        transaction.Complete();
    }
    #endregion

    #region Delete Guest
    public async Task DeleteGuestAsync(long id)
    {
        var target = await Context.Guests.FindAsync(id) ?? throw new NotFoundException();
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        Context.Remove(target);
        await Context.SaveChangesAsync();
        transaction.Complete();
    }
    #endregion

    #region Projection
    internal static GuestDetail ProjectGuest(Guest guest)
    {
        return new GuestDetail
        {
            Id = guest.Id,
            FirstName = guest.FirstName,
            MiddleName = guest.MiddleName,
            LastName = guest.LastName,
            IsMale = guest.IsMale,
            Nationality = guest.Nationality,
            DateOfBirth = guest.DateOfBirth,
            PhoneNumber = guest.PhoneNumber,
            EmailAddress = guest.EmailAddress,
            PassportNumber = guest.PassportNumber
        };
    }
    #endregion

    #region Upsert
    static void UpsertGuest(Guest target, GuestDetail source, GuestDetail? original = null)
    {

        target.FirstName = source.FirstName;
        target.MiddleName = source.MiddleName;
        target.LastName = source.LastName;
        target.IsMale = source.IsMale;
        target.Nationality = source.Nationality;
        target.DateOfBirth = source.DateOfBirth;
        target.PhoneNumber = source.PhoneNumber;
        target.EmailAddress = source.EmailAddress;
        target.PassportNumber = source.PassportNumber;
        if (original != source)
            target.UpdatedAt = DateTime.UtcNow;
    }
    #endregion
}