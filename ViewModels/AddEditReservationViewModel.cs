using System.Collections.ObjectModel;

namespace NovaStayHotel;

internal class AddEditReservationViewModel : BaseAddEditViewModel<ReservationDetail, IReservationService>
{

    #region Construction
    readonly IGuestService guestService;
    readonly IRoomService roomService;
    bool isInitializing = true;

    protected override string EntityName => "Reservation";

    public AddEditReservationViewModel(IReservationService reservationService, IGuestService guestService, IRoomService roomService)
        : base(reservationService)
    {
        this.guestService = guestService;
        this.roomService = roomService;
        InitializeAsync();
    }

    public AddEditReservationViewModel(IReservationService reservationService, IGuestService guestService, IRoomService roomService, ReservationDetail reservationToEdit)
        : base(reservationService, reservationToEdit)
    {
        this.guestService = guestService;
        this.roomService = roomService;
        InitializeAsync();
    }

    async void InitializeAsync()
    {
        await LoadGuestsAndRoomsAsync();
        isInitializing = false;
        ValidateAll();
        UpdateCanSave();
    }
    #endregion

    #region Properties
    bool isActive = true;
    public bool IsActive
    {
        get => isActive;
        set
        {
            if (isActive == value) return;
            isActive = value;
            OnPropertyChanged();
        }
    }

    DateOnly checkInBookingDate = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly CheckInBookingDate
    {
        get => checkInBookingDate;
        set
        {
            if (checkInBookingDate == value) return;
            checkInBookingDate = value;
            OnPropertyChanged();
            if (!isInitializing)
            {
                ValidateProperty(value, nameof(CheckInBookingDate));
                ValidateProperty(CheckOutBookingDate, nameof(CheckOutBookingDate));
                CalculateEstimatedAmounts();
                UpdateCanSave();
            }
        }
    }

    DateOnly checkOutBookingDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    public DateOnly CheckOutBookingDate
    {
        get => checkOutBookingDate;
        set
        {
            if (checkOutBookingDate == value) return;
            checkOutBookingDate = value;
            OnPropertyChanged();
            if (!isInitializing)
            {
                ValidateProperty(value, nameof(CheckOutBookingDate));
                ValidateProperty(CheckInBookingDate, nameof(CheckInBookingDate));
                CalculateEstimatedAmounts();
                UpdateCanSave();
            }
        }
    }

    ReservationStatus status = ReservationStatus.Created;
    public ReservationStatus Status
    {
        get => status;
        set
        {
            if (status == value) return;
            status = value;
            OnPropertyChanged();
            if (!isInitializing)
            {
                ValidateProperty(value, nameof(Status));
                ValidateProperty(CheckedInAt, nameof(CheckedInAt));
                ValidateProperty(CheckedOutAt, nameof(CheckedOutAt));
                ValidateProperty(CanceledAt, nameof(CanceledAt));
                UpdateCanSave();
            }
        }
    }

    DateTime? checkedInAt;
    public DateTime? CheckedInAt
    {
        get => checkedInAt;
        set
        {
            if (checkedInAt == value) return;
            checkedInAt = value;
            OnPropertyChanged();
            if (!isInitializing)
            {
                ValidateProperty(value, nameof(CheckedInAt));
                ValidateProperty(CheckedOutAt, nameof(CheckedOutAt));
                UpdateCanSave();
            }
        }
    }

    DateTime? checkedOutAt;
    public DateTime? CheckedOutAt
    {
        get => checkedOutAt;
        set
        {
            if (checkedOutAt == value) return;
            checkedOutAt = value;
            OnPropertyChanged();
            if (!isInitializing)
            {
                ValidateProperty(value, nameof(CheckedOutAt));
                UpdateCanSave();
            }
        }
    }

    DateTime? canceledAt;
    public DateTime? CanceledAt
    {
        get => canceledAt;
        set
        {
            if (canceledAt == value) return;
            canceledAt = value;
            OnPropertyChanged();
            if (!isInitializing)
            {
                ValidateProperty(value, nameof(CanceledAt));
                UpdateCanSave();
            }
        }
    }

    decimal? discountRate;
    public decimal? DiscountRate
    {
        get => discountRate;
        set
        {
            if (discountRate == value) return;
            discountRate = value;
            OnPropertyChanged();
            if (!isInitializing)
            {
                ValidateProperty(value, nameof(DiscountRate));
                CalculateEstimatedAmounts();
                UpdateCanSave();
            }
        }
    }

    decimal baseAmountUsd;
    public decimal BaseAmountUsd
    {
        get => baseAmountUsd;
        set
        {
            if (baseAmountUsd == value) return;
            baseAmountUsd = value;
            OnPropertyChanged();
        }
    }

    decimal finalAmountUsd;
    public decimal FinalAmountUsd
    {
        get => finalAmountUsd;
        set
        {
            if (finalAmountUsd == value) return;
            finalAmountUsd = value;
            OnPropertyChanged();
        }
    }

    bool isPaid;
    public bool IsPaid
    {
        get => isPaid;
        set
        {
            if (isPaid == value) return;
            isPaid = value;
            OnPropertyChanged();
        }
    }

    PaymentMethod paymentMethod = PaymentMethod.Cash;
    public PaymentMethod PaymentMethod
    {
        get => paymentMethod;
        set
        {
            if (paymentMethod == value) return;
            paymentMethod = value;
            OnPropertyChanged();
        }
    }

    GuestReference? selectedGuest;
    public GuestReference? SelectedGuest
    {
        get => selectedGuest;
        set
        {
            if (selectedGuest == value) return;
            selectedGuest = value;
            OnPropertyChanged();
            if (!isInitializing)
            {
                ValidateProperty(value, nameof(SelectedGuest));
                UpdateCanSave();
            }
        }
    }

    RoomReference? selectedRoom;
    public RoomReference? SelectedRoom
    {
        get => selectedRoom;
        set
        {
            if (selectedRoom == value) return;
            selectedRoom = value;
            OnPropertyChanged();
            if (!isInitializing)
            {
                ValidateProperty(value, nameof(SelectedRoom));
                CalculateEstimatedAmounts();
                UpdateCanSave();
            }
        }
    }

    public ObservableCollection<GuestReference> AvailableGuests { get; } = [];
    public ObservableCollection<RoomReference> AvailableRooms { get; } = [];
    public static Array ReservationStatuses => Enum.GetValues<ReservationStatus>();
    public static Array PaymentMethods => Enum.GetValues<PaymentMethod>();
    public int NumberOfNights => Math.Max(0, CheckOutBookingDate.DayNumber - CheckInBookingDate.DayNumber);
    public string EstimatedCostText => $"Estimated Cost: ${FinalAmountUsd:F2} ({NumberOfNights} nights)";
    #endregion

    #region Methods
    async Task LoadGuestsAndRoomsAsync()
    {
        try
        {
            var guestCriteria = new GuestCriteria();
            var guests = await guestService.GetGuestsAsync(guestCriteria);
            AvailableGuests.Clear();
            foreach (var guest in guests)
            {
                AvailableGuests.Add(new GuestReference
                {
                    Id = guest.Id,
                    Name = guest.FirstName + " " + guest.MiddleName + " " + guest.LastName,
                    PhoneNumber = guest.PhoneNumber
                });
            }
            var roomCriteria = new RoomCriteria { Statuses = { RoomStatus.Available } };
            var rooms = await roomService.GetRoomsAsync(roomCriteria);
            AvailableRooms.Clear();
            foreach (var room in rooms)
            {
                AvailableRooms.Add(new RoomReference
                {
                    Id = room.Id,
                    Number = room.Number,
                    FloorNumber = room.FloorNumber,
                    RoomType = room.Type
                });
            }
        }
        catch (Exception ex)
        {
            if (!propertyErrors.ContainsKey("Loading"))
                propertyErrors["Loading"] = [];
            propertyErrors["Loading"].Add($"Error loading data: {ex.Message}");
            UpdateValidationErrorsCollection();
        }
    }

    async void CalculateEstimatedAmounts()
    {
        if (SelectedRoom == null || NumberOfNights <= 0)
        {
            BaseAmountUsd = 0;
            FinalAmountUsd = 0;
            OnPropertyChanged(nameof(NumberOfNights));
            OnPropertyChanged(nameof(EstimatedCostText));
            return;
        }

        try
        {
            var roomDetails = await roomService.GetRoomAsync(SelectedRoom.Id);
            var baseAmount = roomDetails.PricePerNightUsd * NumberOfNights;
            BaseAmountUsd = baseAmount;
            var finalAmount = baseAmount;
            if (DiscountRate.HasValue && DiscountRate.Value > 0)
            {
                var discountRate = Math.Max(0, Math.Min(100, DiscountRate.Value)) / 100m;
                finalAmount = baseAmount * (1 - discountRate);
            }
            FinalAmountUsd = Math.Round(finalAmount, 2);
            OnPropertyChanged(nameof(NumberOfNights));
            OnPropertyChanged(nameof(EstimatedCostText));
        }
        catch
        {
            BaseAmountUsd = 0;
            FinalAmountUsd = 0;
            OnPropertyChanged(nameof(NumberOfNights));
            OnPropertyChanged(nameof(EstimatedCostText));
        }
    }
    #endregion

    #region Abstract Method Implementations
    protected override void SetDefaultsWithoutValidation()
    {
        isInitializing = true;

        isActive = true;
        checkInBookingDate = DateOnly.FromDateTime(DateTime.Today);
        checkOutBookingDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        status = ReservationStatus.Created;
        checkedInAt = null;
        checkedOutAt = null;
        canceledAt = null;
        discountRate = null;
        baseAmountUsd = 0;
        finalAmountUsd = 0;
        isPaid = false;
        paymentMethod = PaymentMethod.Cash;
        selectedGuest = null;
        selectedRoom = null;

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
        OnPropertyChanged(nameof(SelectedGuest));
        OnPropertyChanged(nameof(SelectedRoom));
    }

    protected override void LoadExistingDataWithoutValidation(ReservationDetail reservationToEdit)
    {
        isInitializing = true;

        isActive = reservationToEdit.IsActive;
        checkInBookingDate = reservationToEdit.CheckInBookingDate;
        checkOutBookingDate = reservationToEdit.CheckOutBookingDate;
        status = reservationToEdit.Status;
        checkedInAt = reservationToEdit.CheckedInAt;
        checkedOutAt = reservationToEdit.CheckedOutAt;
        canceledAt = reservationToEdit.CanceledAt;
        discountRate = reservationToEdit.DiscountRate;
        baseAmountUsd = reservationToEdit.BaseAmountUsd;
        finalAmountUsd = reservationToEdit.FinalAmountUsd;
        isPaid = reservationToEdit.IsPaid;
        paymentMethod = reservationToEdit.PaymentMethod;
        selectedGuest = reservationToEdit.Guest;
        selectedRoom = reservationToEdit.Room;

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
        OnPropertyChanged(nameof(SelectedGuest));
        OnPropertyChanged(nameof(SelectedRoom));
        OnPropertyChanged(nameof(NumberOfNights));
        OnPropertyChanged(nameof(EstimatedCostText));
    }

    protected override void ValidateProperty(object? value, string propertyName)
    {

        if (isInitializing) return;

        ValidatePropertyBase(value, propertyName, errors =>
        {
            switch (propertyName)
            {
                case nameof(CheckInBookingDate):
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    if (CheckInBookingDate < today.AddDays(-1))
                        errors.Add("Check-in date cannot be more than 1 day in the past");
                    if (CheckInBookingDate > today.AddYears(2))
                        errors.Add("Check-in date cannot be more than 2 years in the future");
                    if (CheckInBookingDate >= CheckOutBookingDate)
                        errors.Add("Check-in date must be before check-out date");
                    break;

                case nameof(CheckOutBookingDate):
                    if (CheckOutBookingDate <= CheckInBookingDate)
                        errors.Add("Check-out date must be after check-in date");
                    var reservationLength = CheckOutBookingDate.DayNumber - CheckInBookingDate.DayNumber;
                    if (reservationLength > 365)
                        errors.Add("Reservation cannot exceed 365 days");
                    break;

                case nameof(SelectedGuest):
                    if (SelectedGuest == null)
                        errors.Add("Guest is required");
                    break;

                case nameof(SelectedRoom):
                    if (SelectedRoom == null)
                        errors.Add("Room is required");
                    break;

                case nameof(Status):
                    ValidateStatusRules(errors);
                    break;

                case nameof(DiscountRate):
                    if (DiscountRate.HasValue && (DiscountRate < 0 || DiscountRate > 100))
                        errors.Add("Discount rate must be between 0% and 100%");
                    break;

                case nameof(CheckedInAt):
                    if (Status == ReservationStatus.CheckedIn && CheckedInAt == null)
                        errors.Add("Check-in time is required when status is Checked In");
                    if (CheckedInAt.HasValue && CheckedInAt > DateTime.UtcNow)
                        errors.Add("Check-in time cannot be in the future");
                    break;

                case nameof(CheckedOutAt):
                    if (Status == ReservationStatus.CheckedOut && CheckedOutAt == null)
                        errors.Add("Check-out time is required when status is Checked Out");
                    if (CheckedOutAt.HasValue && CheckedOutAt > DateTime.UtcNow)
                        errors.Add("Check-out time cannot be in the future");
                    if (CheckedInAt.HasValue && CheckedOutAt.HasValue && CheckedOutAt <= CheckedInAt)
                        errors.Add("Check-out time must be after check-in time");
                    break;

                case nameof(CanceledAt):
                    if (Status == ReservationStatus.Canceled && CanceledAt == null)
                        errors.Add("Cancellation time is required when status is Canceled");
                    if (CanceledAt.HasValue && CanceledAt > DateTime.UtcNow)
                        errors.Add("Cancellation time cannot be in the future");
                    break;
            }
        });
    }

    void ValidateStatusRules(List<string> errors)
    {
        switch (Status)
        {
            case ReservationStatus.Created:
                if (CheckedInAt != null || CheckedOutAt != null || CanceledAt != null)
                    errors.Add("Created reservations cannot have check-in, check-out, or cancellation times");
                break;

            case ReservationStatus.CheckedIn:
                if (CheckedInAt == null)
                    errors.Add("Checked-in reservations must have a check-in time");
                if (CheckedOutAt != null || CanceledAt != null)
                    errors.Add("Checked-in reservations cannot have check-out or cancellation times");
                break;

            case ReservationStatus.CheckedOut:
                if (CheckedInAt == null || CheckedOutAt == null)
                    errors.Add("Checked-out reservations must have both check-in and check-out times");
                if (CanceledAt != null)
                    errors.Add("Checked-out reservations cannot have a cancellation time");
                break;

            case ReservationStatus.Canceled:
                if (CanceledAt == null)
                    errors.Add("Canceled reservations must have a cancellation time");
                if (CheckedOutAt != null)
                    errors.Add("Canceled reservations cannot have a check-out time");
                break;
        }
    }

    protected override void ValidateAll()
    {
        if (isInitializing) return;

        propertyErrors.Clear();

        ValidateProperty(CheckInBookingDate, nameof(CheckInBookingDate));
        ValidateProperty(CheckOutBookingDate, nameof(CheckOutBookingDate));
        ValidateProperty(SelectedGuest, nameof(SelectedGuest));
        ValidateProperty(SelectedRoom, nameof(SelectedRoom));
        ValidateProperty(Status, nameof(Status));
        ValidateProperty(DiscountRate, nameof(DiscountRate));
        ValidateProperty(CheckedInAt, nameof(CheckedInAt));
        ValidateProperty(CheckedOutAt, nameof(CheckedOutAt));
        ValidateProperty(CanceledAt, nameof(CanceledAt));
    }

    protected override Task<ReservationDetail> CreateEntityAsync()
    {
        return Task.FromResult(new ReservationDetail
        {
            Id = originalEntityId,
            Guest = SelectedGuest!,
            Room = SelectedRoom!,
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
        });
    }

    protected override async Task UpdateEntityAsync(ReservationDetail entity)
    {
        entity.Validate();
        await service.UpdateReservationAsync(entity);
    }

    protected override async Task AddEntityAsync(ReservationDetail entity)
    {
        entity.Validate();
        await service.AddReservationAsync(entity);
    }

    protected override long GetEntityId(ReservationDetail entity) => entity.Id;
    #endregion
}