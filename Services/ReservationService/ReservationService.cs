using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Helpers.Errors.Model;
using System.Transactions;

namespace NovaStayHotel;

public class ReservationService : IReservationService
{
    #region Construction
    readonly NovaStayHotelDataContext Context;
    public ReservationService(NovaStayHotelDataContext context) => Context = context;
    #endregion

    #region Get Reservations
    public async Task<ReservationDetail> GetReservationAsync(long id)
    {
        return await (from x in Context.Reservations
                      where x.Id == id
                      select ProjectReservation(x)).FirstOrDefaultAsync() ?? throw new NotFoundException();
    }

    public async Task<IEnumerable<ReservationDetail>> GetReservationsAsync(ReservationCriteria criteria)
    {
        criteria.Fixup();
        return await (from x in Context.Reservations
                      where (criteria.Guest == null || x.GuestId == criteria.Guest.Id)
                          && (criteria.Room == null || x.RoomId == criteria.Room.Id)
                          && (criteria.Statuses.IsNullOrEmpty() || criteria.Statuses.Contains(x.Status))
                          && (criteria.IsActive == null || x.IsActive == criteria.IsActive)
                          && (criteria.IsPaid == null || x.IsPaid == criteria.IsPaid)
                          && (criteria.PaymentMethods.IsNullOrEmpty() || criteria.PaymentMethods.Contains(x.PaymentMethod))
                          && (criteria.CheckInBookingFrom == null || x.CheckInBookingDate >= criteria.CheckInBookingFrom)
                          && (criteria.CheckInBookingTo == null || x.CheckInBookingDate <= criteria.CheckInBookingTo)
                          && (criteria.CheckOutBookingFrom == null || x.CheckOutBookingDate >= criteria.CheckOutBookingFrom)
                          && (criteria.CheckOutBookingTo == null || x.CheckOutBookingDate <= criteria.CheckOutBookingTo)
                          && (criteria.CheckedInFrom == null || (x.CheckedInAt != null && x.CheckedInAt >= criteria.CheckedInFrom))
                          && (criteria.CheckedInTo == null || (x.CheckedInAt != null && x.CheckedInAt <= criteria.CheckedInTo))
                          && (criteria.CheckedOutFrom == null || (x.CheckedOutAt != null && x.CheckedOutAt >= criteria.CheckedOutFrom))
                          && (criteria.CheckedOutTo == null || (x.CheckedOutAt != null && x.CheckedOutAt <= criteria.CheckedOutTo))
                          && (criteria.CanceledFrom == null || (x.CanceledAt != null && x.CanceledAt >= criteria.CanceledFrom))
                          && (criteria.CanceledTo == null || (x.CanceledAt != null && x.CanceledAt <= criteria.CanceledTo))
                          && (criteria.MinFinalAmountUsd == null || x.FinalAmountUsd >= criteria.MinFinalAmountUsd)
                          && (criteria.MaxFinalAmountUsd == null || x.FinalAmountUsd <= criteria.MaxFinalAmountUsd)
                      orderby x.Id descending
                      select ProjectReservation(x)).ToListAsync();
    }
    #endregion

    #region Add Reservation
    public async Task<long> AddReservationAsync(ReservationDetail source)
    {
        source.Fixup();
        source.Validate();
        bool reservationExists = await Context.Reservations.AnyAsync(x => x.Id == source.Id);
        if (reservationExists)
            throw new InvalidOperationException("Reservation already exists");
        await EnsureEntitiesExist(source);
        await EnsureNoConflicts(source);
        var calculatedAmounts = await CalculateReservationAmounts(source);
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var target = new Reservation();
        await Context.Reservations.AddAsync(target);
        UpsertReservation(target, source);
        target.BaseAmountUsd = calculatedAmounts.BaseAmount;
        target.FinalAmountUsd = calculatedAmounts.FinalAmount;
        target.CreatedAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        transaction.Complete();
        return target.Id;
    }
    #endregion

    #region Update Reservation
    public async Task UpdateReservationAsync(ReservationDetail source)
    {
        source.Fixup();
        source.Validate();
        var target = await Context.Reservations
            .FirstOrDefaultAsync(x => x.Id == source.Id) ?? throw new NotFoundException();
        var original = ProjectReservation(target);
        await EnsureEntitiesExist(source);
        await EnsureNoConflicts(source);
        bool needsRecalculation = target.CheckInBookingDate != source.CheckInBookingDate ||
                                 target.CheckOutBookingDate != source.CheckOutBookingDate ||
                                 target.RoomId != source.Room.Id ||
                                 target.DiscountRate != source.DiscountRate;
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        UpsertReservation(target, source, original);
        if (needsRecalculation)
        {
            var calculatedAmounts = await CalculateReservationAmounts(source);
            target.BaseAmountUsd = calculatedAmounts.BaseAmount;
            target.FinalAmountUsd = calculatedAmounts.FinalAmount;
        }

        await Context.SaveChangesAsync();
        transaction.Complete();
    }
    #endregion

    #region Delete Reservation
    public async Task DeleteReservationAsync(long id)
    {
        var target = await Context.Reservations.FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException();
        if (target.CheckedInAt != null && target.CheckedOutAt == null && target.CanceledAt == null)
            throw new InvalidOperationException("Cannot delete a reservation where the guest is currently checked in");
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        Context.Reservations.Remove(target);
        await Context.SaveChangesAsync();
        transaction.Complete();
    }
    #endregion

    #region Amount Calculation
    /// <summary>
    /// calculate the base amount and final amount according to the number of nights and discount rate
    /// </summary>
    /// <param name="reservation"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    async Task<(decimal BaseAmount, decimal FinalAmount)> CalculateReservationAmounts(ReservationDetail reservation)
    {
        var room = await Context.Rooms
            .FirstOrDefaultAsync(r => r.Id == reservation.Room.Id)
            ?? throw new NotFoundException("Room not found for amount calculation");
        int numberOfNights = reservation.CheckOutBookingDate.DayNumber - reservation.CheckInBookingDate.DayNumber;
        if (numberOfNights <= 0)
            throw new InvalidOperationException("Invalid date range for amount calculation");
        decimal baseAmount = room.PricePerNightUsd * numberOfNights;
        decimal finalAmount = baseAmount;
        if (reservation.DiscountRate.HasValue && reservation.DiscountRate.Value > 0)
        {
            decimal discountRate = Math.Max(0, Math.Min(100, reservation.DiscountRate.Value)) / 100m;
            finalAmount = baseAmount * (1 - discountRate);
        }
        baseAmount = Math.Round(baseAmount, 2);
        finalAmount = Math.Round(finalAmount, 2);
        return (baseAmount, finalAmount);
    }
    #endregion

    #region Conflict Detection
    /// <summary>
    ///  Makes sure there are no conflicts with existing reservations for the same room and guest and that business rules are followed.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    async Task EnsureNoConflicts(ReservationDetail source)
    {
        await ValidateRoomAvailability(source);
        await ValidateNoDateOverlaps(source);
        await ValidateGuestConstraints(source);
        ValidateBusinessRules(source);
        if (source.Id != 0)
            await ValidateStateTransitions(source);
    }

    async Task ValidateRoomAvailability(ReservationDetail source)
    {
        var room = await Context.Rooms.FirstOrDefaultAsync(x => x.Id == source.Room.Id) ?? throw new NotFoundException("Room not found");
        if (room.Status != RoomStatus.Available)
            throw new InvalidOperationException("Room is not available for booking");
        if (room.Status == RoomStatus.OutOfService)
            throw new InvalidOperationException("Room is out of service");
    }

    async Task ValidateNoDateOverlaps(ReservationDetail source)
    {
        var conflictingReservations = await Context.Reservations
            .Where(x => x.Id != source.Id &&
                       x.RoomId == source.Room.Id &&
                       x.IsActive &&
                       (x.Status == ReservationStatus.Created ||
                        x.Status == ReservationStatus.CheckedIn))
            .ToListAsync();

        foreach (var existing in conflictingReservations)
        {
            bool hasDateOverlap = source.CheckInBookingDate < existing.CheckOutBookingDate &&
                                  source.CheckOutBookingDate > existing.CheckInBookingDate;

            if (hasDateOverlap)
            {
                if (source.CheckInBookingDate == existing.CheckOutBookingDate ||
                    source.CheckOutBookingDate == existing.CheckInBookingDate)
                {
                    if (existing.CheckedInAt != null && existing.CheckedOutAt == null && existing.CanceledAt == null)
                        throw new InvalidOperationException($"Room is occupied until guest checks out");
                    continue;
                }
                throw new InvalidOperationException(
                    $"Room is already reserved from {existing.CheckInBookingDate:yyyy-MM-dd} to {existing.CheckOutBookingDate:yyyy-MM-dd} " +
                    $"Status: {existing.Status})");
            }
        }
    }
    async Task ValidateGuestConstraints(ReservationDetail source)
    {
        var guest = await Context.Guests
            .FirstOrDefaultAsync(x => x.Id == source.Guest.Id)
            ?? throw new NotFoundException("Guest not found");
        var overlappingRoomReservations = await Context.Reservations
            .Where(x => x.Id != source.Id &&
                       x.RoomId == source.Room.Id &&
                       x.IsActive &&
                       (x.Status == ReservationStatus.Created ||
                        x.Status == ReservationStatus.CheckedIn) &&
                       source.CheckInBookingDate < x.CheckOutBookingDate &&
                       source.CheckOutBookingDate > x.CheckInBookingDate)
            .AnyAsync();
        if (overlappingRoomReservations)
            throw new InvalidOperationException("This room is already reserved during the selected period");
    }


    static void ValidateBusinessRules(ReservationDetail source)
    {
        var now = DateTime.UtcNow.Date;
        var today = DateOnly.FromDateTime(now);
        if (source.CheckInBookingDate < today.AddDays(-1))
            throw new InvalidOperationException("Cannot create reservations for dates more than 1 day in the past");
        if (source.CheckInBookingDate > today.AddYears(2))
            throw new InvalidOperationException("Cannot create reservations more than 2 years in advance");
        var reservationLength = source.CheckOutBookingDate.DayNumber - source.CheckInBookingDate.DayNumber;
        if (reservationLength > 365)
            throw new InvalidOperationException("Reservation cannot exceed 365 days");
        if (reservationLength < 1)
            throw new InvalidOperationException("Reservation must be at least 1 day long");
        ValidateStatusSpecificRules(source);
    }

    static void ValidateStatusSpecificRules(ReservationDetail source)
    {
        switch (source.Status)
        {
            case ReservationStatus.Created:
                if (source.CheckedInAt != null || source.CheckedOutAt != null || source.CanceledAt != null)
                    throw new InvalidOperationException("Created reservations cannot have check-in, check-out, or cancellation dates");
                break;

            case ReservationStatus.CheckedIn:
                if (source.CheckedInAt == null)
                    throw new InvalidOperationException("Checked-in reservations must have a check-in date");
                if (source.CheckedOutAt != null || source.CanceledAt != null)
                    throw new InvalidOperationException("Checked-in reservations cannot have check-out or cancellation dates");
                break;

            case ReservationStatus.CheckedOut:
                if (source.CheckedInAt == null || source.CheckedOutAt == null)
                    throw new InvalidOperationException("Checked-out reservations must have both check-in and check-out dates");
                if (source.CanceledAt != null)
                    throw new InvalidOperationException("Checked-out reservations cannot have a cancellation date");
                break;

            case ReservationStatus.Canceled:
                if (source.CanceledAt == null)
                    throw new InvalidOperationException("Canceled reservations must have a cancellation date");
                if (source.CheckedOutAt != null)
                    throw new InvalidOperationException("Canceled reservations cannot have a check-out date");
                break;
        }
    }

    async Task ValidateStateTransitions(ReservationDetail source)
    {
        var existing = await Context.Reservations.FirstOrDefaultAsync(x => x.Id == source.Id);
        if (existing == null) return;
        var validTransitions = new Dictionary<ReservationStatus, ReservationStatus[]>
        {
            [ReservationStatus.Created] = [ReservationStatus.CheckedIn, ReservationStatus.Canceled],
            [ReservationStatus.CheckedIn] = [ReservationStatus.CheckedOut, ReservationStatus.Canceled],
            [ReservationStatus.CheckedOut] = [],
            [ReservationStatus.Canceled] = []
        };

        if (existing.Status != source.Status)
        {
            if (!validTransitions.ContainsKey(existing.Status) ||
                !validTransitions[existing.Status].Contains(source.Status))
            {
                throw new InvalidOperationException(
                    $"Invalid status transition from {existing.Status} to {source.Status}");
            }
        }
        if (!existing.IsActive && source.IsActive)
        {
            if (existing.Status == ReservationStatus.CheckedOut || existing.Status == ReservationStatus.Canceled)
                throw new InvalidOperationException("Cannot reactivate completed or canceled reservations");
        }
    }
    #endregion

    #region Entity Existence Validation
    /// <summary>
    /// Ensures that Guest and Room exist in the database
    /// </summary>
    async Task EnsureEntitiesExist(ReservationDetail source)
    {
        // Check if Guest exists
        var guestExists = await Context.Guests.AnyAsync(g => g.Id == source.Guest.Id);
        if (!guestExists)
            throw new NotFoundException($"Guest not found");

        // Check if Room exists
        var roomExists = await Context.Rooms.AnyAsync(r => r.Id == source.Room.Id);
        if (!roomExists)
            throw new NotFoundException($"Room not found");
    }
    #endregion

    #region Projection
    internal static ReservationDetail ProjectReservation(Reservation x)
    {
        return new ReservationDetail
        {
            Id = x.Id,
            Guest = GuestReference.ProjectGuestReference(x.Guest),
            Room = RoomReference.ProjectRoomReference(x.Room),
            IsActive = x.IsActive,
            CheckInBookingDate = x.CheckInBookingDate,
            CheckOutBookingDate = x.CheckOutBookingDate,
            Status = x.Status,
            CheckedInAt = x.CheckedInAt,
            CheckedOutAt = x.CheckedOutAt,
            CanceledAt = x.CanceledAt,
            DiscountRate = x.DiscountRate,
            BaseAmountUsd = x.BaseAmountUsd,
            FinalAmountUsd = x.FinalAmountUsd,
            IsPaid = x.IsPaid,
            PaymentMethod = x.PaymentMethod
        };
    }
    #endregion

    #region Upsert
    static void UpsertReservation(Reservation target, ReservationDetail source, ReservationDetail? original = null)
    {
        target.GuestId = source.Guest.Id;
        target.RoomId = source.Room.Id;
        target.IsActive = source.IsActive;
        target.CheckInBookingDate = source.CheckInBookingDate;
        target.CheckOutBookingDate = source.CheckOutBookingDate;
        target.Status = source.Status;
        target.CheckedInAt = source.CheckedInAt;
        target.CheckedOutAt = source.CheckedOutAt;
        target.CanceledAt = source.CanceledAt;
        target.DiscountRate = source.DiscountRate;
        target.BaseAmountUsd = source.BaseAmountUsd;
        target.FinalAmountUsd = source.FinalAmountUsd;
        target.IsPaid = source.IsPaid;
        target.PaymentMethod = source.PaymentMethod;

        if (original != source)
            target.UpdatedAt = DateTime.UtcNow;
    }
    #endregion
}