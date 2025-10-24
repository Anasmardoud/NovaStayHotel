using Microsoft.Extensions.DependencyInjection;
using NovaStayHotel.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Data;
using System.Windows.Input;

namespace NovaStayHotel;

public class SelectableItem<T>(T value, string? displayName = null) : INotifyPropertyChanged
{
    bool isSelected;
    public T Value { get; set; } = value;
    public string DisplayName { get; set; } = displayName ?? value?.ToString() ?? "";
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
}

public class RoomListViewModel : BaseViewModel
{
    #region Construction
    readonly IRoomService roomService;
    readonly IServiceProvider serviceProvider;
    readonly NavigationService<HomeViewModel> homeNavService;
    public RoomListViewModel(
    IRoomService roomService,
    IServiceProvider serviceProvider,
    NavigationService<HomeViewModel> homeNavService)
    {
        this.roomService = roomService;
        this.serviceProvider = serviceProvider;
        this.homeNavService = homeNavService;
        InitializeMultiSelectCollections();
        RoomsView = CollectionViewSource.GetDefaultView(Rooms);
        RoomsView.Filter = FilterRooms;
        NavigateToHomeCommand = new NavigateCommand<HomeViewModel>(homeNavService);
        LoadRoomsCommand = new LoadRoomsCommand(this);
        AddRoomCommand = new AddRoomCommand(this, serviceProvider);
        EditRoomCommand = new EditRoomCommand(this, serviceProvider);
        DeleteRoomCommand = new DeleteRoomCommand(this, roomService);
        SetMaintenanceCommand = new SetMaintenanceCommand(this, roomService);
        CompleteMaintenanceCommand = new CompleteMaintenanceCommand(this, roomService);
        ClearFiltersCommand = new ClearFiltersCommand(this);
        RefreshCommand = new RefreshCommand(this);
        SearchCommand = new SearchCommand(this);
        LoadRoomsCommand.Execute(null);
    }
    #endregion

    #region Properties
    public ObservableCollection<RoomDetailViewModel> Rooms { get; set; } = [];
    public ICollectionView RoomsView { get; set; }
    public ObservableCollection<SelectableItem<RoomType>> RoomTypes { get; set; }
    public ObservableCollection<SelectableItem<RoomStatus>> RoomStatuses { get; set; }

    int? minRoomNumber;
    [Range(RoomDetail.MinNumber, RoomDetail.MaxNumber, ErrorMessage = "Room number must be between 1 and 200")]
    public int? MinRoomNumber
    {
        get => minRoomNumber;
        set
        {
            if (value.HasValue && (value < RoomDetail.MinNumber || value > RoomDetail.MaxNumber))
                return;
            minRoomNumber = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }
    int? maxRoomNumber;
    [Range(RoomDetail.MinNumber, RoomDetail.MaxNumber, ErrorMessage = "Room number must be between 1 and 200")]
    public int? MaxRoomNumber
    {
        get => maxRoomNumber;
        set
        {
            if (value.HasValue && (value < RoomDetail.MinNumber || value > RoomDetail.MaxNumber))
                return;
            maxRoomNumber = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }

    int? minFloorNumber;
    [Range(RoomDetail.MinFloor, RoomDetail.MaxFloor, ErrorMessage = "Floor number must be between 0 and 20")]
    public int? MinFloorNumber
    {
        get => minFloorNumber;
        set
        {
            if (value.HasValue && (value < RoomDetail.MinFloor || value > RoomDetail.MaxFloor))
                return;
            minFloorNumber = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }

    int? maxFloorNumber;
    [Range(RoomDetail.MinFloor, RoomDetail.MaxFloor, ErrorMessage = "Floor number must be between 0 and 20")]
    public int? MaxFloorNumber
    {
        get => maxFloorNumber;
        set
        {
            if (value.HasValue && (value < RoomDetail.MinFloor || value > RoomDetail.MaxFloor))
                return;
            maxFloorNumber = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }

    bool? hasBalcony;
    public bool? HasBalcony
    {
        get => hasBalcony;
        set
        {
            hasBalcony = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }

    decimal? minPrice;
    [Range(typeof(decimal), "0", "100000", ErrorMessage = "Price must be between $0 and $100,000")]
    public decimal? MinPrice
    {
        get => minPrice;
        set
        {
            if (value.HasValue && (value < RoomDetail.MinUsdPrice || value > RoomDetail.MaxUsdPrice))
                return;
            minPrice = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }

    decimal? maxPrice;
    [Range(typeof(decimal), "0", "100000", ErrorMessage = "Price must be between $0 and $100,000")]
    public decimal? MaxPrice
    {
        get => maxPrice;
        set
        {
            if (value.HasValue && (value < RoomDetail.MinUsdPrice || value > RoomDetail.MaxUsdPrice))
                return;
            maxPrice = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }

    int hasBalconySelectedIndex;
    public int HasBalconySelectedIndex
    {
        get => hasBalconySelectedIndex;
        set
        {
            hasBalconySelectedIndex = value;
            OnPropertyChanged();
            HasBalcony = value switch
            {
                1 => true,
                2 => false,
                _ => null
            };
        }
    }

    RoomDetailViewModel? selectedRoom;
    public RoomDetailViewModel? SelectedRoom
    {
        get => selectedRoom;
        set
        {
            selectedRoom = value;
            OnPropertyChanged();
            EditRoomCommand.OnCanExecuteChanged();
            DeleteRoomCommand.OnCanExecuteChanged();
            SetMaintenanceCommand.OnCanExecuteChanged();
            CompleteMaintenanceCommand.OnCanExecuteChanged();
        }
    }

    bool isLoading;
    public bool IsLoading
    {
        get => isLoading;
        set
        {
            isLoading = value;
            OnPropertyChanged();
        }
    }

    string activeFiltersText = "";
    public string ActiveFiltersText
    {
        get => activeFiltersText;
        set
        {
            activeFiltersText = value;
            OnPropertyChanged();
        }
    }

    bool hasActiveFilters;
    public bool HasActiveFilters
    {
        get => hasActiveFilters;
        set
        {
            hasActiveFilters = value;
            OnPropertyChanged();
        }
    }

    int filteredRoomsCount;
    public int FilteredRoomsCount
    {
        get => filteredRoomsCount;
        set
        {
            filteredRoomsCount = value;
            OnPropertyChanged();
        }
    }

    public static string[] BalconyOptions => ["All", "With Balcony", "Without Balcony"];
    public static int[] FloorOptions => [.. Enumerable.Range(0, 21)];
    public IEnumerable<RoomType> SelectedTypes => RoomTypes.Where(x => x.IsSelected).Select(x => x.Value);
    public IEnumerable<RoomStatus> SelectedStatuses => RoomStatuses.Where(x => x.IsSelected).Select(x => x.Value);
    #endregion

    #region Commands
    public ICommand NavigateToHomeCommand { get; }
    public AsyncCommandBase LoadRoomsCommand { get; }
    public AsyncCommandBase AddRoomCommand { get; }
    public AsyncCommandBase EditRoomCommand { get; }
    public AsyncCommandBase DeleteRoomCommand { get; }
    public AsyncCommandBase SetMaintenanceCommand { get; }
    public AsyncCommandBase CompleteMaintenanceCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public AsyncCommandBase RefreshCommand { get; }
    public AsyncCommandBase SearchCommand { get; }
    #endregion

    #region Methods
    void InitializeMultiSelectCollections()
    {
        RoomTypes = [];
        foreach (var type in Enum.GetValues<RoomType>())
        {
            var item = new SelectableItem<RoomType>(type, type.ToString());
            item.PropertyChanged += (s, e) => UpdateActiveFilters();
            RoomTypes.Add(item);
        }
        RoomStatuses = [];
        foreach (var status in Enum.GetValues<RoomStatus>())
        {
            var displayName = status switch
            {
                RoomStatus.UnderMaintenance => "Under Maintenance",
                RoomStatus.OutOfService => "Out of Service",
                _ => status.ToString()
            };
            var item = new SelectableItem<RoomStatus>(status, displayName);
            item.PropertyChanged += (s, e) => UpdateActiveFilters();
            RoomStatuses.Add(item);
        }
    }

    public async Task LoadRoomsAsync()
    {
        try
        {
            IsLoading = true;
            var criteria = new RoomCriteria
            {
                MinRoomNumber = MinRoomNumber,
                MaxRoomNumber = MaxRoomNumber,
                MinFloorNumber = MinFloorNumber,
                MaxFloorNumber = MaxFloorNumber,
                HasBalcony = HasBalcony,
                MinPricePerNightUsd = MinPrice,
                MaxPricePerNightUsd = MaxPrice
            };
            foreach (var type in SelectedTypes)
                criteria.Types.Add(type);
            foreach (var status in SelectedStatuses)
                criteria.Statuses.Add(status);
            criteria.Fixup();
            var rooms = await roomService.GetRoomsAsync(criteria);
            Rooms.Clear();
            foreach (var room in rooms)
                Rooms.Add(new RoomDetailViewModel(room, roomService));
            FilteredRoomsCount = Rooms.Count;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error loading rooms: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    bool FilterRooms(object item) => true;
    void UpdateActiveFilters()
    {
        var filters = new List<string>();
        if (MinRoomNumber.HasValue || MaxRoomNumber.HasValue)
        {
            var min = MinRoomNumber?.ToString() ?? "1";
            var max = MaxRoomNumber?.ToString() ?? "200";
            filters.Add($"Room: {min}-{max}");
        }
        if (MinFloorNumber.HasValue || MaxFloorNumber.HasValue)
        {
            var min = MinFloorNumber?.ToString() ?? "0";
            var max = MaxFloorNumber?.ToString() ?? "20";
            filters.Add($"Floor: {min}-{max}");
        }
        if (MinPrice.HasValue || MaxPrice.HasValue)
        {
            var min = MinPrice?.ToString("F0") ?? "0";
            var max = MaxPrice?.ToString("F0") ?? "100000";
            filters.Add($"Price: ${min}-${max}");
        }
        var selectedTypeNames = SelectedTypes.Select(t => t.ToString()).ToList();
        if (selectedTypeNames.Count != 0)
        {
            filters.Add($"Types: {string.Join(", ", selectedTypeNames)}");
        }
        var selectedStatusNames = SelectedStatuses.Select(s =>
            s == RoomStatus.UnderMaintenance ? "Under Maintenance" :
            s == RoomStatus.OutOfService ? "Out of Service" :
            s.ToString()).ToList();
        if (selectedStatusNames.Count != 0)
        {
            filters.Add($"Status: {string.Join(", ", selectedStatusNames)}");
        }
        if (HasBalcony.HasValue)
        {
            filters.Add(HasBalcony.Value ? "With Balcony" : "Without Balcony");
        }
        ActiveFiltersText = string.Join(" | ", filters);
        HasActiveFilters = filters.Count != 0;
    }

    public void ClearFilters()
    {
        MinRoomNumber = null;
        MaxRoomNumber = null;
        MinFloorNumber = null;
        MaxFloorNumber = null;
        foreach (var type in RoomTypes)
            type.IsSelected = false;
        foreach (var status in RoomStatuses)
            status.IsSelected = false;
        HasBalconySelectedIndex = 0;
        MinPrice = null;
        MaxPrice = null;
        UpdateActiveFilters();
    }
    public static bool IsValidRoomNumber(int? number) => !number.HasValue || (number >= RoomDetail.MinNumber && number <= RoomDetail.MaxNumber);

    public static bool IsValidFloorNumber(int? floor) => !floor.HasValue || (floor >= RoomDetail.MinFloor && floor <= RoomDetail.MaxFloor);

    public static bool IsValidPrice(decimal? price) => !price.HasValue || (price >= RoomDetail.MinUsdPrice && price <= RoomDetail.MaxUsdPrice);
    public string GetValidationMessage(string propertyName)
    {
        return propertyName switch
        {
            nameof(MinRoomNumber) or nameof(MaxRoomNumber) => $"Room number must be between {RoomDetail.MinNumber} and {RoomDetail.MaxNumber}",
            nameof(MinFloorNumber) or nameof(MaxFloorNumber) => $"Floor number must be between {RoomDetail.MinFloor} and {RoomDetail.MaxFloor}",
            nameof(MinPrice) or nameof(MaxPrice) => $"Price must be between ${RoomDetail.MinUsdPrice:F0} and ${RoomDetail.MaxUsdPrice:F0}",
            _ => ""
        };
    }
    #endregion
}

#region Commands Implementation
public class LoadRoomsCommand(RoomListViewModel viewModel) : AsyncCommandBase
{
    readonly RoomListViewModel viewModel = viewModel;
    public override async Task ExecuteAsync(object? parameter) => await viewModel.LoadRoomsAsync();
}

public class AddRoomCommand(RoomListViewModel viewModel, IServiceProvider serviceProvider) : AsyncCommandBase
{
    readonly RoomListViewModel viewModel = viewModel;
    readonly IServiceProvider serviceProvider = serviceProvider;
    public override async Task ExecuteAsync(object? parameter)
    {
        var viewModelFactory = serviceProvider.GetRequiredService<Func<AddEditRoomViewModel>>();
        var addRoomViewModel = viewModelFactory();
        var dialog = new AddEditRoomWindow { DataContext = addRoomViewModel };
        if (dialog.ShowDialog() == true)
            await viewModel.LoadRoomsAsync();
    }
}

public class EditRoomCommand(RoomListViewModel viewModel, IServiceProvider serviceProvider) : AsyncCommandBase
{
    readonly RoomListViewModel viewModel = viewModel;
    readonly IServiceProvider serviceProvider = serviceProvider;
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) && viewModel.SelectedRoom != null;
    public override async Task ExecuteAsync(object? parameter)
    {
        if (viewModel.SelectedRoom == null) return;
        try
        {
            var roomDetail = new RoomDetail
            {
                Id = viewModel.SelectedRoom.Id,
                Number = viewModel.SelectedRoom.Number,
                FloorNumber = viewModel.SelectedRoom.FloorNumber,
                Type = viewModel.SelectedRoom.Type,
                Status = viewModel.SelectedRoom.Status,
                HasBalcony = viewModel.SelectedRoom.HasBalcony,
                PricePerNightUsd = viewModel.SelectedRoom.PricePerNightUsd,
                LastMaintainedAt = viewModel.SelectedRoom.LastMaintainedAt,
                Description = viewModel.SelectedRoom.Description
            };
            var editRoomViewModel = new AddEditRoomViewModel(serviceProvider.GetRequiredService<IRoomService>(), roomDetail);
            var dialog = new AddEditRoomWindow { DataContext = editRoomViewModel };
            if (dialog.ShowDialog() == true)
                await viewModel.LoadRoomsAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error editing room: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}

public class DeleteRoomCommand(RoomListViewModel viewModel, IRoomService roomService) : AsyncCommandBase
{
    readonly RoomListViewModel viewModel = viewModel;
    readonly IRoomService roomService = roomService;
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) && viewModel.SelectedRoom != null;
    public override async Task ExecuteAsync(object? parameter)
    {
        if (viewModel.SelectedRoom == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete room {viewModel.SelectedRoom.Number}?",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                await roomService.DeleteRoomAsync(viewModel.SelectedRoom.Id);
                viewModel.Rooms.Remove(viewModel.SelectedRoom);
                viewModel.FilteredRoomsCount = viewModel.Rooms.Count;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error deleting room: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
public class SetMaintenanceCommand(RoomListViewModel viewModel, IRoomService roomService) : AsyncCommandBase
{
    readonly RoomListViewModel viewModel = viewModel;
    readonly IRoomService roomService = roomService;
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) && viewModel.SelectedRoom != null &&
        viewModel.SelectedRoom.Status != RoomStatus.UnderMaintenance;
    public override async Task ExecuteAsync(object? parameter)
    {
        if (viewModel.SelectedRoom == null) return;
        var result = System.Windows.MessageBox.Show(
            $"Set room {viewModel.SelectedRoom.Number} to maintenance mode?",
            "Set Maintenance",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                viewModel.IsLoading = true;
                var updatedRoom = new RoomDetail
                {
                    Id = viewModel.SelectedRoom.Id,
                    Number = viewModel.SelectedRoom.Number,
                    FloorNumber = viewModel.SelectedRoom.FloorNumber,
                    Type = viewModel.SelectedRoom.Type,
                    Status = RoomStatus.UnderMaintenance,
                    HasBalcony = viewModel.SelectedRoom.HasBalcony,
                    PricePerNightUsd = viewModel.SelectedRoom.PricePerNightUsd,
                    LastMaintainedAt = viewModel.SelectedRoom.LastMaintainedAt,
                    Description = viewModel.SelectedRoom.Description
                };
                using (var scope = new System.Transactions.TransactionScope(
                    System.Transactions.TransactionScopeOption.Suppress,
                    System.Transactions.TransactionScopeAsyncFlowOption.Enabled))
                {
                    await roomService.UpdateRoomAsync(updatedRoom);
                    scope.Complete();
                }
                var roomNumber = viewModel.SelectedRoom.Number;
                await viewModel.LoadRoomsAsync();
                System.Windows.MessageBox.Show(
                    $"Room {roomNumber} has been set to maintenance mode.",
                    "Maintenance Set",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error setting maintenance: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                viewModel.IsLoading = false;
            }
        }
    }
}

public class CompleteMaintenanceCommand(RoomListViewModel viewModel, IRoomService roomService) : AsyncCommandBase
{
    readonly RoomListViewModel viewModel = viewModel;
    readonly IRoomService roomService = roomService;
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) && viewModel.SelectedRoom != null &&
        viewModel.SelectedRoom.Status == RoomStatus.UnderMaintenance;
    public override async Task ExecuteAsync(object? parameter)
    {
        if (viewModel.SelectedRoom == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Complete maintenance for room {viewModel.SelectedRoom.Number}?\n\nThis will set the room status to 'Available' and update the maintenance date.",
            "Complete Maintenance",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                viewModel.IsLoading = true;
                var updatedRoom = new RoomDetail
                {
                    Id = viewModel.SelectedRoom.Id,
                    Number = viewModel.SelectedRoom.Number,
                    FloorNumber = viewModel.SelectedRoom.FloorNumber,
                    Type = viewModel.SelectedRoom.Type,
                    Status = RoomStatus.Available,
                    HasBalcony = viewModel.SelectedRoom.HasBalcony,
                    PricePerNightUsd = viewModel.SelectedRoom.PricePerNightUsd,
                    LastMaintainedAt = DateTime.UtcNow,
                    Description = viewModel.SelectedRoom.Description
                };
                using (var scope = new System.Transactions.TransactionScope(
                    System.Transactions.TransactionScopeOption.Suppress,
                    System.Transactions.TransactionScopeAsyncFlowOption.Enabled))
                {
                    await roomService.UpdateRoomAsync(updatedRoom);
                    scope.Complete();
                }
                var roomNumber = viewModel.SelectedRoom.Number;
                await viewModel.LoadRoomsAsync();
                System.Windows.MessageBox.Show(
                    $"Maintenance completed for room {roomNumber}.\nRoom is now available.",
                    "Maintenance Completed",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error completing maintenance: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                viewModel.IsLoading = false;
            }
        }
    }
}
public class ClearFiltersCommand(RoomListViewModel viewModel) : CommandBase
{
    readonly RoomListViewModel viewModel = viewModel;

    public override void Execute(object? parameter) => viewModel.ClearFilters();
}
public class RefreshCommand(RoomListViewModel viewModel) : AsyncCommandBase
{
    readonly RoomListViewModel viewModel = viewModel;

    public override async Task ExecuteAsync(object? parameter) => await viewModel.LoadRoomsAsync();
}
public class SearchCommand(RoomListViewModel viewModel) : AsyncCommandBase
{
    readonly RoomListViewModel viewModel = viewModel;

    public override async Task ExecuteAsync(object? parameter) => await viewModel.LoadRoomsAsync();
}
#endregion