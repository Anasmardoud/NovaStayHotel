using System.ComponentModel;
using System.Windows.Input;

namespace NovaStayHotel;

public class ReservationDetailViewModel : BaseViewModel, IEditableObject
{
    #region Construction
    readonly IReservationService reservationService;
    ReservationDetail currentReservation;
    ReservationDetail? backupReservation;
    public ReservationDetailViewModel(ReservationDetail reservation, IReservationService reservationService)
    {
        this.reservationService = reservationService;
        currentReservation = reservation;
        UpdatePropertiesFromReservation(reservation);
        HasChanges = false;
        SaveCommand = new SaveReservationCommand(this);
        CancelCommand = new CancelEditReservationCommand(this);
    }
    #endregion

    #region Properties
    public long Id => currentReservation.Id;

    GuestReference guest;
    public GuestReference Guest
    {
        get => guest;
        set
        {
            guest = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(GuestDisplayName));
            HasChanges = true;
        }
    }

    RoomReference room;
    public RoomReference Room
    {
        get => room;
        set
        {
            room = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RoomDisplayName));
            HasChanges = true;
        }
    }

    bool isActive;
    public bool IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    DateOnly checkInBookingDate;
    public DateOnly CheckInBookingDate
    {
        get => checkInBookingDate;
        set
        {
            checkInBookingDate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CheckInBookingDateFormatted));
            HasChanges = true;
        }
    }

    DateOnly checkOutBookingDate;
    public DateOnly CheckOutBookingDate
    {
        get => checkOutBookingDate;
        set
        {
            checkOutBookingDate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CheckOutBookingDateFormatted));
            HasChanges = true;
        }
    }

    ReservationStatus status;
    public ReservationStatus Status
    {
        get => status;
        set
        {
            status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusFormatted));
            HasChanges = true;
        }
    }

    DateTime? checkedInAt;
    public DateTime? CheckedInAt
    {
        get => checkedInAt;
        set
        {
            checkedInAt = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    DateTime? checkedOutAt;
    public DateTime? CheckedOutAt
    {
        get => checkedOutAt;
        set
        {
            checkedOutAt = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    DateTime? canceledAt;
    public DateTime? CanceledAt
    {
        get => canceledAt;
        set
        {
            canceledAt = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    decimal? discountRate;
    public decimal? DiscountRate
    {
        get => discountRate;
        set
        {
            discountRate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DiscountRateFormatted));
            HasChanges = true;
        }
    }

    decimal baseAmountUsd;
    public decimal BaseAmountUsd
    {
        get => baseAmountUsd;
        set
        {
            baseAmountUsd = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    decimal finalAmountUsd;
    public decimal FinalAmountUsd
    {
        get => finalAmountUsd;
        set
        {
            finalAmountUsd = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FinalAmountUsdFormatted));
            HasChanges = true;
        }
    }

    bool isPaid;
    public bool IsPaid
    {
        get => isPaid;
        set
        {
            isPaid = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    PaymentMethod paymentMethod;
    public PaymentMethod PaymentMethod
    {
        get => paymentMethod;
        set
        {
            paymentMethod = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PaymentMethodFormatted));
            HasChanges = true;
        }
    }
    public string CheckInBookingDateFormatted => CheckInBookingDate.ToString("yyyy-MM-dd");
    public string CheckOutBookingDateFormatted => CheckOutBookingDate.ToString("yyyy-MM-dd");
    public string FinalAmountUsdFormatted => $"${FinalAmountUsd:F2}";
    public string StatusFormatted => Status switch
    {
        ReservationStatus.CheckedIn => "Checked In",
        ReservationStatus.CheckedOut => "Checked Out",
        _ => Status.ToString()
    };
    public string PaymentMethodFormatted => PaymentMethod switch
    {
        PaymentMethod.EWallet => "E-Wallet",
        PaymentMethod.BankTransfer => "Bank Transfer",
        PaymentMethod.CryptoCurrency => "Cryptocurrency",
        _ => PaymentMethod.ToString()
    };
    public string DiscountRateFormatted => DiscountRate?.ToString("F1") + "%" ?? "None";
    public string GuestDisplayName => Guest?.Name ?? "";
    public string RoomDisplayName => $"Room {Room?.Number}";

    bool isEditing;
    public bool IsEditing
    {
        get => isEditing;
        set
        {
            isEditing = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsReadOnly));
        }
    }

    public bool IsReadOnly => !IsEditing;

    bool hasChanges;
    public bool HasChanges
    {
        get => hasChanges;
        set
        {
            hasChanges = value;
            OnPropertyChanged();
        }
    }

    bool isSaving;
    public bool IsSaving
    {
        get => isSaving;
        set
        {
            isSaving = value;
            OnPropertyChanged();
        }
    }
    public Array ReservationStatuses => Enum.GetValues<ReservationStatus>();
    public Array PaymentMethods => Enum.GetValues<PaymentMethod>();
    #endregion

    #region Commands
    public AsyncCommandBase SaveCommand { get; }
    public ICommand CancelCommand { get; }
    #endregion

    #region Methods
    void UpdatePropertiesFromReservation(ReservationDetail reservation)
    {
        guest = reservation.Guest;
        room = reservation.Room;
        isActive = reservation.IsActive;
        checkInBookingDate = reservation.CheckInBookingDate;
        checkOutBookingDate = reservation.CheckOutBookingDate;
        status = reservation.Status;
        checkedInAt = reservation.CheckedInAt;
        checkedOutAt = reservation.CheckedOutAt;
        canceledAt = reservation.CanceledAt;
        discountRate = reservation.DiscountRate;
        baseAmountUsd = reservation.BaseAmountUsd;
        finalAmountUsd = reservation.FinalAmountUsd;
        isPaid = reservation.IsPaid;
        paymentMethod = reservation.PaymentMethod;
        OnPropertyChanged(nameof(Guest));
        OnPropertyChanged(nameof(Room));
        OnPropertyChanged(nameof(IsActive));
        OnPropertyChanged(nameof(CheckInBookingDate));
        OnPropertyChanged(nameof(CheckOutBookingDate));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(CheckedInAt));
        OnPropertyChanged(nameof(CheckedOutAt));
        OnPropertyChanged(nameof(CanceledAt));
        OnPropertyChanged(nameof(DiscountRate));
        OnPropertyChanged(nameof(BaseAmountUsd));
        OnPropertyChanged(nameof(FinalAmountUsd));
        OnPropertyChanged(nameof(IsPaid));
        OnPropertyChanged(nameof(PaymentMethod));
        OnPropertyChanged(nameof(CheckInBookingDateFormatted));
        OnPropertyChanged(nameof(CheckOutBookingDateFormatted));
        OnPropertyChanged(nameof(FinalAmountUsdFormatted));
        OnPropertyChanged(nameof(StatusFormatted));
        OnPropertyChanged(nameof(PaymentMethodFormatted));
        OnPropertyChanged(nameof(DiscountRateFormatted));
        OnPropertyChanged(nameof(GuestDisplayName));
        OnPropertyChanged(nameof(RoomDisplayName));
    }

    public void StartEdit()
    {
        if (!IsEditing)
            BeginEdit();
    }
    public async Task SaveAsync()
    {
        if (!IsEditing || !HasChanges) return;
        try
        {
            IsSaving = true;
            var updatedReservation = new ReservationDetail
            {
                Id = Id,
                Guest = Guest,
                Room = Room,
                IsActive = IsActive,
                CheckInBookingDate = CheckInBookingDate,
                CheckOutBookingDate = CheckOutBookingDate,
                Status = Status,
                CheckedInAt = CheckedInAt,
                CheckedOutAt = CheckedOutAt,
                CanceledAt = CanceledAt,
                DiscountRate = DiscountRate,
                BaseAmountUsd = BaseAmountUsd,
                FinalAmountUsd = FinalAmountUsd,
                IsPaid = IsPaid,
                PaymentMethod = PaymentMethod
            };
            await reservationService.UpdateReservationAsync(updatedReservation);
            currentReservation = updatedReservation;
            EndEdit();
            HasChanges = false;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error saving reservation: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    public void CancelEditCommandAction()
    {
        if (IsEditing)
            ((IEditableObject)this).CancelEdit();
    }
    #endregion

    #region IEditableObject Implementation
    public void BeginEdit()
    {
        if (backupReservation == null)
        {
            backupReservation = new ReservationDetail
            {
                Id = Id,
                Guest = Guest,
                Room = Room,
                IsActive = IsActive,
                CheckInBookingDate = CheckInBookingDate,
                CheckOutBookingDate = CheckOutBookingDate,
                Status = Status,
                CheckedInAt = CheckedInAt,
                CheckedOutAt = CheckedOutAt,
                CanceledAt = CanceledAt,
                DiscountRate = DiscountRate,
                BaseAmountUsd = BaseAmountUsd,
                FinalAmountUsd = FinalAmountUsd,
                IsPaid = IsPaid,
                PaymentMethod = PaymentMethod
            };
        }
        IsEditing = true;
    }

    void IEditableObject.CancelEdit()
    {
        if (backupReservation != null)
        {
            UpdatePropertiesFromReservation(backupReservation);
            backupReservation = null;
            HasChanges = false;
        }
        IsEditing = false;
    }

    public void EndEdit()
    {
        backupReservation = null;
        IsEditing = false;
    }
    #endregion
}

#region Commands Implementation
public class SaveReservationCommand(ReservationDetailViewModel viewModel) : AsyncCommandBase
{
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) && viewModel.IsEditing && viewModel.HasChanges;
    public override async Task ExecuteAsync(object? parameter) => await viewModel.SaveAsync();
}

public class CancelEditReservationCommand(ReservationDetailViewModel viewModel) : CommandBase
{
    public override bool CanExecute(object? parameter) => viewModel.IsEditing;
    public override void Execute(object? parameter) => viewModel.CancelEditCommandAction();
}
#endregion