using System.ComponentModel;
using System.Windows.Input;

namespace NovaStayHotel;

public class RoomDetailViewModel : BaseViewModel, IEditableObject
{
    #region Construction
    readonly IRoomService roomService;
    RoomDetail currentRoom;
    RoomDetail? backupRoom;
    public RoomDetailViewModel(RoomDetail room, IRoomService roomService)
    {
        this.roomService = roomService;
        currentRoom = room;
        UpdatePropertiesFromRoom(room);
        HasChanges = false;
        SaveCommand = new SaveRoomCommand(this);
        CancelCommand = new CancelEditCommand(this);
    }
    #endregion

    #region Properties
    public long Id => currentRoom.Id;

    int number;
    public int Number
    {
        get => number;
        set
        {
            number = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    int floorNumber;
    public int FloorNumber
    {
        get => floorNumber;
        set
        {
            floorNumber = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    RoomType type;
    public RoomType Type
    {
        get => type;
        set
        {
            type = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    RoomStatus status;
    public RoomStatus Status
    {
        get => status;
        set
        {
            status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusColor));
            HasChanges = true;
        }
    }

    bool hasBalcony;
    public bool HasBalcony
    {
        get => hasBalcony;
        set
        {
            hasBalcony = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    decimal pricePerNightUsd;
    public decimal PricePerNightUsd
    {
        get => pricePerNightUsd;
        set
        {
            pricePerNightUsd = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PricePerNightUsdFormatted));
            HasChanges = true;
        }
    }

    public string PricePerNightUsdFormatted => $"${PricePerNightUsd:F2}";

    DateTime? lastMaintainedAt;
    public DateTime? LastMaintainedAt
    {
        get => lastMaintainedAt;
        set
        {
            lastMaintainedAt = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LastMaintainedAtFormatted));
            HasChanges = true;
        }
    }

    public string LastMaintainedAtFormatted =>
        LastMaintainedAt?.ToString("yyyy-MM-dd") ?? "Never";

    string? description;
    public string? Description
    {
        get => description;
        set
        {
            description = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

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
    public string StatusColor => Status switch
    {
        RoomStatus.Available => "Green",
        RoomStatus.Occupied => "Red",
        RoomStatus.Reserved => "Orange",
        RoomStatus.UnderMaintenance => "Yellow",
        RoomStatus.OutOfService => "Gray",
        _ => "Black"
    };
    public static Array RoomTypes => Enum.GetValues<RoomType>();
    public static Array RoomStatuses => Enum.GetValues<RoomStatus>();
    #endregion

    #region Commands
    public AsyncCommandBase SaveCommand { get; }
    public ICommand CancelCommand { get; }
    #endregion

    #region Methods
    void UpdatePropertiesFromRoom(RoomDetail room)
    {
        number = room.Number;
        floorNumber = room.FloorNumber;
        type = room.Type;
        status = room.Status;
        hasBalcony = room.HasBalcony;
        pricePerNightUsd = room.PricePerNightUsd;
        lastMaintainedAt = room.LastMaintainedAt;
        description = room.Description;
        OnPropertyChanged(nameof(Number));
        OnPropertyChanged(nameof(FloorNumber));
        OnPropertyChanged(nameof(Type));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(HasBalcony));
        OnPropertyChanged(nameof(PricePerNightUsd));
        OnPropertyChanged(nameof(PricePerNightUsdFormatted));
        OnPropertyChanged(nameof(LastMaintainedAt));
        OnPropertyChanged(nameof(LastMaintainedAtFormatted));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(StatusColor));
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
            var updatedRoom = new RoomDetail
            {
                Id = Id,
                Number = Number,
                FloorNumber = FloorNumber,
                Type = Type,
                Status = Status,
                HasBalcony = HasBalcony,
                PricePerNightUsd = PricePerNightUsd,
                LastMaintainedAt = LastMaintainedAt,
                Description = Description
            };
            await roomService.UpdateRoomAsync(updatedRoom);
            currentRoom = updatedRoom;
            EndEdit();
            HasChanges = false;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error saving room: {ex.Message}", "Error",
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
        if (backupRoom == null)
        {
            backupRoom = new RoomDetail
            {
                Id = Id,
                Number = Number,
                FloorNumber = FloorNumber,
                Type = Type,
                Status = Status,
                HasBalcony = HasBalcony,
                PricePerNightUsd = PricePerNightUsd,
                LastMaintainedAt = LastMaintainedAt,
                Description = Description
            };
        }
        IsEditing = true;
    }

    void IEditableObject.CancelEdit()
    {
        if (backupRoom != null)
        {
            UpdatePropertiesFromRoom(backupRoom);
            backupRoom = null;
            HasChanges = false;
        }
        IsEditing = false;
    }

    public void EndEdit()
    {
        backupRoom = null;
        IsEditing = false;
    }
    #endregion
}

#region Commands Implementation
public class SaveRoomCommand(RoomDetailViewModel viewModel) : AsyncCommandBase
{
    readonly RoomDetailViewModel viewModel = viewModel;
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) && viewModel.IsEditing && viewModel.HasChanges;
    public override async Task ExecuteAsync(object? parameter) => await viewModel.SaveAsync();
}

public class CancelEditCommand(RoomDetailViewModel viewModel) : CommandBase
{
    readonly RoomDetailViewModel viewModel = viewModel;
    public override bool CanExecute(object? parameter) => viewModel.IsEditing;
    public override void Execute(object? parameter) => viewModel.CancelEditCommandAction();
}
#endregion
