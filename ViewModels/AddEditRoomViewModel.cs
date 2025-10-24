namespace NovaStayHotel;

public class AddEditRoomViewModel : BaseAddEditViewModel<RoomDetail, IRoomService>
{


    #region Construction
    protected override string EntityName => "Room";
    public AddEditRoomViewModel(IRoomService roomService)
        : base(roomService) { }

    public AddEditRoomViewModel(IRoomService roomService, RoomDetail roomToEdit)
        : base(roomService, roomToEdit) { }
    #endregion

    #region Properties
    int number = 101;
    public int Number
    {
        get => number;
        set
        {
            if (number == value) return;
            number = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(Number));
            UpdateCanSave();
        }
    }

    int floorNumber = 1;
    public int FloorNumber
    {
        get => floorNumber;
        set
        {
            if (floorNumber == value) return;
            floorNumber = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(FloorNumber));
            UpdateCanSave();
        }
    }

    RoomType type = RoomType.Single;
    public RoomType Type
    {
        get => type;
        set
        {
            if (type == value) return;
            type = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(Type));
            UpdateCanSave();
        }
    }

    RoomStatus status = RoomStatus.Available;
    public RoomStatus Status
    {
        get => status;
        set
        {
            if (status == value) return;
            status = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(Status));
            UpdateCanSave();
        }
    }

    bool hasBalcony;
    public bool HasBalcony
    {
        get => hasBalcony;
        set
        {
            if (hasBalcony == value) return;
            hasBalcony = value;
            OnPropertyChanged();
        }
    }

    decimal pricePerNightUsd = 100m;
    public decimal PricePerNightUsd
    {
        get => pricePerNightUsd;
        set
        {
            if (pricePerNightUsd == value) return;
            pricePerNightUsd = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(PricePerNightUsd));
            UpdateCanSave();
        }
    }

    DateTime? lastMaintainedAt;
    public DateTime? LastMaintainedAt
    {
        get => lastMaintainedAt;
        set
        {
            if (lastMaintainedAt == value) return;
            lastMaintainedAt = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(LastMaintainedAt));
            UpdateCanSave();
        }
    }

    string? description;
    public string? Description
    {
        get => description;
        set
        {
            if (description == value) return;
            description = value;
            OnPropertyChanged();
        }
    }

    public static Array RoomTypes => Enum.GetValues<RoomType>();
    public static Array RoomStatuses => Enum.GetValues<RoomStatus>();
    #endregion

    #region Abstract Method Implementations
    protected override void SetDefaultsWithoutValidation()
    {
        number = 101;
        floorNumber = 1;
        type = RoomType.Single;
        status = RoomStatus.Available;
        pricePerNightUsd = 100m;
        hasBalcony = false;
        lastMaintainedAt = null;
        description = null;

        OnPropertyChanged(nameof(Number));
        OnPropertyChanged(nameof(FloorNumber));
        OnPropertyChanged(nameof(Type));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(HasBalcony));
        OnPropertyChanged(nameof(PricePerNightUsd));
        OnPropertyChanged(nameof(LastMaintainedAt));
        OnPropertyChanged(nameof(Description));
    }

    protected override void LoadExistingDataWithoutValidation(RoomDetail roomToEdit)
    {
        number = roomToEdit.Number;
        floorNumber = roomToEdit.FloorNumber;
        type = roomToEdit.Type;
        status = roomToEdit.Status;
        hasBalcony = roomToEdit.HasBalcony;
        pricePerNightUsd = roomToEdit.PricePerNightUsd;
        lastMaintainedAt = roomToEdit.LastMaintainedAt;
        description = roomToEdit.Description;

        OnPropertyChanged(nameof(Number));
        OnPropertyChanged(nameof(FloorNumber));
        OnPropertyChanged(nameof(Type));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(HasBalcony));
        OnPropertyChanged(nameof(PricePerNightUsd));
        OnPropertyChanged(nameof(LastMaintainedAt));
        OnPropertyChanged(nameof(Description));
    }

    protected override void ValidateProperty(object? value, string propertyName)
    {
        ValidatePropertyBase(value, propertyName, errors =>
        {
            switch (propertyName)
            {
                case nameof(Number):
                    if (Number < RoomDetail.MinNumber || Number > RoomDetail.MaxNumber)
                        errors.Add($"Room number must be between {RoomDetail.MinNumber} and {RoomDetail.MaxNumber}");
                    break;

                case nameof(FloorNumber):
                    if (FloorNumber < RoomDetail.MinFloor || FloorNumber > RoomDetail.MaxFloor)
                        errors.Add($"Floor number must be between {RoomDetail.MinFloor} and {RoomDetail.MaxFloor}");
                    break;

                case nameof(Type):
                    if (Type == default)
                        errors.Add("Room type is required");
                    break;

                case nameof(Status):
                    if (Status == default)
                        errors.Add("Room status is required");
                    break;

                case nameof(PricePerNightUsd):
                    if (PricePerNightUsd < RoomDetail.MinUsdPrice || PricePerNightUsd > RoomDetail.MaxUsdPrice)
                        errors.Add($"Price must be between ${RoomDetail.MinUsdPrice:#.##} and ${RoomDetail.MaxUsdPrice:#.##}");
                    break;

                case nameof(LastMaintainedAt):
                    if (LastMaintainedAt != null && LastMaintainedAt > DateTime.UtcNow)
                        errors.Add("Maintenance date cannot be in the future");
                    break;
            }
        });
    }

    protected override void ValidateAll()
    {
        propertyErrors.Clear();

        ValidateProperty(Number, nameof(Number));
        ValidateProperty(FloorNumber, nameof(FloorNumber));
        ValidateProperty(Type, nameof(Type));
        ValidateProperty(Status, nameof(Status));
        ValidateProperty(PricePerNightUsd, nameof(PricePerNightUsd));
        ValidateProperty(LastMaintainedAt, nameof(LastMaintainedAt));
    }

    protected override Task<RoomDetail> CreateEntityAsync()
    {
        return Task.FromResult(new RoomDetail
        {
            Id = originalEntityId,
            Number = Number,
            FloorNumber = FloorNumber,
            Type = Type,
            Status = Status,
            HasBalcony = HasBalcony,
            PricePerNightUsd = PricePerNightUsd,
            LastMaintainedAt = LastMaintainedAt,
            Description = Description
        });
    }

    protected override async Task UpdateEntityAsync(RoomDetail entity)
    {
        entity.Validate();
        await service.UpdateRoomAsync(entity);
    }

    protected override async Task AddEntityAsync(RoomDetail entity)
    {
        entity.Validate();
        await service.AddRoomAsync(entity);
    }

    protected override long GetEntityId(RoomDetail entity) => entity.Id;
    #endregion
}