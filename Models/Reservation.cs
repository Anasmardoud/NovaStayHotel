// Ignore Spelling: Usd Fixup
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace NovaStayHotel;

public class Reservation
{
    #region Properties
    [Key]
    public long Id { get; set; }

    [ForeignKey(nameof(Room))]
    public long RoomId { get; set; }
    public virtual Room Room { get; set; } = null!;

    [ForeignKey(nameof(Guest))]
    public long GuestId { get; set; }
    public virtual Guest Guest { get; set; } = null!;

    public bool IsActive { get; set; }
    public DateOnly CheckInBookingDate { get; set; }
    public DateOnly CheckOutBookingDate { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public DateTime? CanceledAt { get; set; }
    public decimal? DiscountRate { get; set; }
    public decimal BaseAmountUsd { get; set; }
    public decimal FinalAmountUsd { get; set; }
    public bool IsPaid { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    #endregion
}

public record ReservationDetail
{
    #region Properties
    public long Id { get; init; }
    public required GuestReference Guest { get; init; }
    public required RoomReference Room { get; init; }
    public required bool IsActive { get; init; }
    public required DateOnly CheckInBookingDate { get; init; }
    public required DateOnly CheckOutBookingDate { get; init; }
    public required ReservationStatus Status { get; init; }
    public required DateTime? CheckedInAt { get; init; }
    public required DateTime? CheckedOutAt { get; init; }
    public required DateTime? CanceledAt { get; init; }
    public required decimal? DiscountRate { get; init; }
    public required decimal BaseAmountUsd { get; init; }
    public required decimal FinalAmountUsd { get; init; }
    public required bool IsPaid { get; init; }
    public required PaymentMethod PaymentMethod { get; init; }
    #endregion

    #region Fixup & Validation

    public void Fixup() => this.FixupStringProperties();

    public const decimal MinAmountUsd = 0m;
    public const int MinPercentage = 0;
    public const int MaxPercentage = 100;

    public void Validate()
    {
        ValidateReferences();
        ValidateBookingDates();
        ValidateAmounts();
        ValidatePayment();
        ValidateStateDates();
        ValidateDiscountRate();
    }

    #region Validation Sections

    private void ValidateReferences()
    {
        if (Guest == null || Guest.Id == default)
            throw new ArgumentException("Guest is required");

        if (Room == null || Room.Id == default)
            throw new ArgumentException("Room is required");
    }

    private void ValidateBookingDates()
    {
        if (CheckInBookingDate == default)
            throw new ArgumentException("Check-in booking date is required");

        if (CheckOutBookingDate == default)
            throw new ArgumentException("Check-out booking date is required");

        if (CheckOutBookingDate <= CheckInBookingDate)
            throw new ArgumentException("Check-out booking date must come after check-in booking date");
    }

    private void ValidateAmounts()
    {
        if (BaseAmountUsd < MinAmountUsd)
            throw new ArgumentException("Base amount must be non-negative");

        if (FinalAmountUsd < MinAmountUsd)
            throw new ArgumentException("Final amount must be non-negative");
    }

    private void ValidatePayment()
    {
        if (PaymentMethod == default)
            throw new ArgumentException("Payment method is required");
    }

    private void ValidateStateDates()
    {
        DateTime now = DateTime.UtcNow;

        if (CheckedInAt == null && (CheckedOutAt != null || CanceledAt != null))
            throw new InvalidOperationException("Cannot check out or cancel without checking in first");

        if (CheckedInAt != null && CheckedInAt > now)
            throw new ArgumentException("Checked-in-at cannot be set in the future");

        if (CheckedOutAt != null && CheckedOutAt > now)
            throw new ArgumentException("Checked-out-at cannot be set in the future");

        if (CanceledAt != null && CanceledAt > now)
            throw new ArgumentException("Canceled-at cannot be set in the future");

        if (CheckedOutAt != null && CanceledAt != null)
            throw new ArgumentException("A booking cannot be both checked out and canceled");

        if (CheckedInAt != null && CheckedOutAt != null && CheckedOutAt < CheckedInAt)
            throw new ArgumentException("Checked out date cannot be earlier than checked in date");

        if (CheckedInAt != null && CanceledAt != null && CanceledAt < CheckedInAt)
            throw new ArgumentException("Canceled date cannot be earlier than checked in date");
    }
    void ValidateDiscountRate()
    {
        if (DiscountRate != null)
        {
            if (DiscountRate < MinPercentage || DiscountRate > MaxPercentage)
                throw new ArgumentOutOfRangeException("Discount rate must be between 0 and 100 (percentage)");
        }
    }
    #endregion

    #endregion
}

public record ReservationReference
{
    #region Properties
    public required long Id { get; init; }
    public required ReservationStatus Status { get; init; }
    #endregion
}

public record ReservationCriteria
{
    #region Properties
    public GuestReference? Guest { get; init; }
    public RoomReference? Room { get; init; }
    public HashSet<ReservationStatus> Statuses { get; init; } = [];
    public bool? IsActive { get; init; }
    public bool? IsPaid { get; init; }
    public HashSet<PaymentMethod> PaymentMethods { get; init; } = [];
    public DateOnly? CheckInBookingFrom { get; init; }
    public DateOnly? CheckInBookingTo { get; init; }
    public DateOnly? CheckOutBookingFrom { get; init; }
    public DateOnly? CheckOutBookingTo { get; init; }
    public DateTime? CheckedInFrom { get; init; }
    public DateTime? CheckedInTo { get; init; }
    public DateTime? CheckedOutFrom { get; init; }
    public DateTime? CheckedOutTo { get; init; }
    public DateTime? CanceledFrom { get; init; }
    public DateTime? CanceledTo { get; init; }
    public decimal? MinFinalAmountUsd { get; init; }
    public decimal? MaxFinalAmountUsd { get; init; }
    #endregion

    #region Fixup
    public void Fixup() => this.FixupStringProperties();
    #endregion
}
