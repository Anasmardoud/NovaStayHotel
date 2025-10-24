using Microsoft.Extensions.DependencyInjection;
using NovaStayHotel.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace NovaStayHotel;

public class ReservationListViewModel : BaseViewModel
{
    #region Construction
    readonly IReservationService reservationService;
    readonly IGuestService guestService;
    readonly IRoomService roomService;
    readonly IServiceProvider serviceProvider;
    readonly NavigationService<HomeViewModel> homeNavService;

    bool filtersActive = false;
    public ReservationListViewModel(
    IReservationService reservationService,
    IGuestService guestService,
    IRoomService roomService,
    IServiceProvider serviceProvider,
    NavigationService<HomeViewModel> homeNavService)
    {
        this.reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
        this.guestService = guestService ?? throw new ArgumentNullException(nameof(guestService));
        this.roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.homeNavService = homeNavService ?? throw new ArgumentNullException(nameof(homeNavService));
        InitializeMultiSelectCollections();
        ReservationsView = CollectionViewSource.GetDefaultView(Reservations);
        ReservationsView.Filter = FilterReservations;
        NavigateToHomeCommand = new NavigateCommand<HomeViewModel>(homeNavService);
        LoadReservationsCommand = new LoadReservationsCommand(this);
        AddReservationCommand = new AddReservationCommand(this, serviceProvider);
        EditReservationCommand = new EditReservationCommand(this, serviceProvider);
        DeleteReservationCommand = new DeleteReservationCommand(this, reservationService);
        CheckInCommand = new CheckInCommand(this, reservationService);
        CheckOutCommand = new CheckOutCommand(this, reservationService);
        CancelReservationCommand = new CancelReservationCommand(this, reservationService);
        ClearFiltersCommand = new ClearFiltersReservationCommand(this);
        RefreshCommand = new RefreshReservationCommand(this);
        SearchCommand = new SearchReservationCommand(this);
        InitializeAsync();
    }
    #endregion

    #region Properties
    public ObservableCollection<ReservationDetailViewModel> Reservations { get; set; } = [];
    public ICollectionView ReservationsView { get; set; }
    public ObservableCollection<SelectableItem<ReservationStatus>> ReservationStatuses { get; set; } = [];
    public ObservableCollection<SelectableItem<PaymentMethod>> PaymentMethods { get; set; } = [];
    public ObservableCollection<GuestReference> AvailableGuests { get; set; } = [];
    public ObservableCollection<RoomReference> AvailableRooms { get; set; } = [];
    GuestReference? selectedGuest;
    public GuestReference? SelectedGuest
    {
        get => selectedGuest;
        set
        {
            selectedGuest = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }
    RoomReference? selectedRoom;
    public RoomReference? SelectedRoom
    {
        get => selectedRoom;
        set
        {
            selectedRoom = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }
    bool? isActive;
    public bool? IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }
    bool? isPaid;
    public bool? IsPaid
    {
        get => isPaid;
        set
        {
            isPaid = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }
    DateOnly? checkInBookingFrom;
    public DateOnly? CheckInBookingFrom
    {
        get => checkInBookingFrom;
        set
        {
            checkInBookingFrom = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }
    DateOnly? checkInBookingTo;
    public DateOnly? CheckInBookingTo
    {
        get => checkInBookingTo;
        set
        {
            checkInBookingTo = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }
    DateOnly? checkOutBookingFrom;
    public DateOnly? CheckOutBookingFrom
    {
        get => checkOutBookingFrom;
        set
        {
            checkOutBookingFrom = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }
    DateOnly? checkOutBookingTo;
    public DateOnly? CheckOutBookingTo
    {
        get => checkOutBookingTo;
        set
        {
            checkOutBookingTo = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }
    decimal? minAmount;
    public decimal? MinAmount
    {
        get => minAmount;
        set
        {
            minAmount = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }
    decimal? maxAmount;
    public decimal? MaxAmount
    {
        get => maxAmount;
        set
        {
            maxAmount = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }
    ReservationDetailViewModel? selectedReservation;
    public ReservationDetailViewModel? SelectedReservation
    {
        get => selectedReservation;
        set
        {
            selectedReservation = value;
            OnPropertyChanged();
            UpdateCommandStates();
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
    int filteredReservationsCount;
    public int FilteredReservationsCount
    {
        get => filteredReservationsCount;
        set
        {
            filteredReservationsCount = value;
            OnPropertyChanged();
        }
    }
    public static string[] ActiveOptions => ["All", "Active Only", "Inactive Only"];
    public static string[] PaidOptions => ["All", "Paid", "Unpaid"];
    int activeSelectedIndex;
    public int ActiveSelectedIndex
    {
        get => activeSelectedIndex;
        set
        {
            activeSelectedIndex = value;
            OnPropertyChanged();
            IsActive = value switch
            {
                1 => true,
                2 => false,
                _ => null
            };
        }
    }
    int paidSelectedIndex;
    public int PaidSelectedIndex
    {
        get => paidSelectedIndex;
        set
        {
            paidSelectedIndex = value;
            OnPropertyChanged();
            IsPaid = value switch
            {
                1 => true,
                2 => false,
                _ => null
            };
        }
    }
    public IEnumerable<ReservationStatus> SelectedStatuses =>
        ReservationStatuses?.Where(x => x?.IsSelected == true).Select(x => x.Value) ?? Enumerable.Empty<ReservationStatus>();
    public IEnumerable<PaymentMethod> SelectedPaymentMethods =>
        PaymentMethods?.Where(x => x?.IsSelected == true).Select(x => x.Value) ?? Enumerable.Empty<PaymentMethod>();
    #endregion

    #region Commands
    public ICommand NavigateToHomeCommand { get; }
    public AsyncCommandBase LoadReservationsCommand { get; }
    public AsyncCommandBase AddReservationCommand { get; }
    public AsyncCommandBase EditReservationCommand { get; }
    public AsyncCommandBase DeleteReservationCommand { get; }
    public AsyncCommandBase CheckInCommand { get; }
    public AsyncCommandBase CheckOutCommand { get; }
    public AsyncCommandBase CancelReservationCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public AsyncCommandBase RefreshCommand { get; }
    public AsyncCommandBase SearchCommand { get; }
    #endregion


    #region Helper Methods
    void UpdateCommandStates()
    {
        EditReservationCommand.OnCanExecuteChanged();
        DeleteReservationCommand.OnCanExecuteChanged();
        CheckInCommand.OnCanExecuteChanged();
        CheckOutCommand.OnCanExecuteChanged();
        CancelReservationCommand.OnCanExecuteChanged();
    }
    public void UpdateFilteredReservationsCount()
    {
        if (ReservationsView != null)
        {
            var filteredItems = ReservationsView.Cast<ReservationDetailViewModel>().ToList();
            FilteredReservationsCount = filteredItems.Count;
        }
    }
    #endregion

    #region Methods
    async void InitializeAsync()
    {
        try
        {
            await LoadGuestsAndRoomsAsync();
            await LoadReservationsAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error initializing: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    void InitializeMultiSelectCollections()
    {
        ReservationStatuses.Clear();
        foreach (var status in Enum.GetValues<ReservationStatus>())
        {
            var displayName = status switch
            {
                ReservationStatus.CheckedIn => "Checked In",
                ReservationStatus.CheckedOut => "Checked Out",
                _ => status.ToString()
            };
            var item = new SelectableItem<ReservationStatus>(status, displayName);
            item.PropertyChanged += (s, e) => UpdateActiveFilters();
            ReservationStatuses.Add(item);
        }
        PaymentMethods.Clear();
        foreach (var method in Enum.GetValues<PaymentMethod>())
        {
            var displayName = method switch
            {
                PaymentMethod.EWallet => "E-Wallet",
                PaymentMethod.BankTransfer => "Bank Transfer",
                PaymentMethod.CryptoCurrency => "Cryptocurrency",
                _ => method.ToString()
            };
            var item = new SelectableItem<PaymentMethod>(method, displayName);
            item.PropertyChanged += (s, e) => UpdateActiveFilters();
            PaymentMethods.Add(item);
        }
    }

    async Task LoadGuestsAndRoomsAsync()
    {
        try
        {
            var guestCriteria = new GuestCriteria();
            var guestsResult = await guestService.GetGuestsAsync(guestCriteria);
            AvailableGuests.Clear();
            AvailableGuests.Add(new GuestReference { Id = 0, Name = "All Guests", PhoneNumber = "" });

            if (guestsResult != null)
            {
                foreach (var guest in guestsResult)
                {
                    var fullName = $"{guest.FirstName ?? ""} {guest.MiddleName ?? ""} {guest.LastName ?? ""}".Trim();
                    AvailableGuests.Add(new GuestReference
                    {
                        Id = guest.Id,
                        Name = string.IsNullOrWhiteSpace(fullName) ? "Unknown Guest" : fullName,
                        PhoneNumber = guest.PhoneNumber ?? ""
                    });
                }
            }
            var roomCriteria = new RoomCriteria();
            var roomsResult = await roomService.GetRoomsAsync(roomCriteria);
            AvailableRooms.Clear();
            AvailableRooms.Add(new RoomReference { Id = 0, Number = 0, FloorNumber = 0, RoomType = default });

            if (roomsResult != null)
            {
                foreach (var room in roomsResult)
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
            SelectedGuest = AvailableGuests.FirstOrDefault();
            SelectedRoom = AvailableRooms.FirstOrDefault();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error loading filter data: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    public async Task LoadReservationsAsync()
    {
        try
        {
            IsLoading = true;
            var criteria = new ReservationCriteria();
            criteria.Fixup();
            var reservationsResult = await reservationService.GetReservationsAsync(criteria);
            Reservations.Clear();
            if (reservationsResult != null)
            {
                foreach (var reservation in reservationsResult)
                {
                    try
                    {
                        Reservations.Add(new ReservationDetailViewModel(reservation, reservationService));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading reservation {reservation?.Id}: {ex.Message}");
                    }
                }
            }
            filtersActive = false;
            ReservationsView?.Refresh();
            UpdateFilteredReservationsCount();
            UpdateActiveFilters();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error loading reservations: {ex.Message}", "Error",
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    bool FilterReservations(object item)
    {
        if (!filtersActive)
            return true;
        if (item is not ReservationDetailViewModel reservation)
            return false;
        try
        {
            if (SelectedGuest?.Id > 0)
            {
                if (reservation.Guest?.Id != SelectedGuest.Id)
                    return false;
            }
            if (SelectedRoom?.Id > 0)
            {
                if (reservation.Room?.Id != SelectedRoom.Id)
                    return false;
            }
            if (IsActive.HasValue)
            {
                if (reservation.IsActive != IsActive.Value)
                    return false;
            }
            if (IsPaid.HasValue)
            {
                if (reservation.IsPaid != IsPaid.Value)
                    return false;
            }
            if (CheckInBookingFrom.HasValue && reservation.CheckInBookingDate < CheckInBookingFrom.Value)
                return false;
            if (CheckInBookingTo.HasValue && reservation.CheckInBookingDate > CheckInBookingTo.Value)
                return false;
            if (CheckOutBookingFrom.HasValue && reservation.CheckOutBookingDate < CheckOutBookingFrom.Value)
                return false;
            if (CheckOutBookingTo.HasValue && reservation.CheckOutBookingDate > CheckOutBookingTo.Value)
                return false;
            if (MinAmount.HasValue && reservation.FinalAmountUsd < MinAmount.Value)
                return false;
            if (MaxAmount.HasValue && reservation.FinalAmountUsd > MaxAmount.Value)
                return false;
            var selectedStatuses = SelectedStatuses.ToList();
            if (selectedStatuses.Count != 0 && !selectedStatuses.Contains(reservation.Status))
                return false;
            var selectedMethods = SelectedPaymentMethods.ToList();
            if (selectedMethods.Count != 0 && !selectedMethods.Contains(reservation.PaymentMethod))
                return false;
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in FilterReservations: {ex.Message}");
            return true;
        }
    }
    public void ApplyFilters()
    {
        try
        {
            filtersActive = true;
            ReservationsView?.Refresh();
            UpdateFilteredReservationsCount();
            UpdateActiveFilters();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error applying filters: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    void UpdateActiveFilters()
    {
        var filters = new List<string>();

        try
        {
            if (SelectedGuest?.Id > 0)
                filters.Add($"Guest: {SelectedGuest.Name ?? "Unknown"}");
            if (SelectedRoom?.Id > 0)
                filters.Add($"Room: {SelectedRoom.Number}");
            if (CheckInBookingFrom.HasValue || CheckInBookingTo.HasValue)
            {
                var from = CheckInBookingFrom?.ToString("yyyy-MM-dd") ?? "Any";
                var to = CheckInBookingTo?.ToString("yyyy-MM-dd") ?? "Any";
                filters.Add($"Check-in: {from} to {to}");
            }
            if (CheckOutBookingFrom.HasValue || CheckOutBookingTo.HasValue)
            {
                var from = CheckOutBookingFrom?.ToString("yyyy-MM-dd") ?? "Any";
                var to = CheckOutBookingTo?.ToString("yyyy-MM-dd") ?? "Any";
                filters.Add($"Check-out: {from} to {to}");
            }
            if (MinAmount.HasValue || MaxAmount.HasValue)
            {
                var min = MinAmount?.ToString("F2") ?? "0";
                var max = MaxAmount?.ToString("F2") ?? "∞";
                filters.Add($"Amount: ${min}-${max}");
            }
            var selectedStatusNames = SelectedStatuses.Select(s => s switch
            {
                ReservationStatus.CheckedIn => "Checked In",
                ReservationStatus.CheckedOut => "Checked Out",
                _ => s.ToString()
            }).ToList();
            if (selectedStatusNames.Any())
                filters.Add($"Status: {string.Join(", ", selectedStatusNames)}");
            var selectedMethodNames = SelectedPaymentMethods.Select(m => m switch
            {
                PaymentMethod.EWallet => "E-Wallet",
                PaymentMethod.BankTransfer => "Bank Transfer",
                PaymentMethod.CryptoCurrency => "Cryptocurrency",
                _ => m.ToString()
            }).ToList();
            if (selectedMethodNames.Any())
                filters.Add($"Payment: {string.Join(", ", selectedMethodNames)}");
            if (IsActive.HasValue)
                filters.Add(IsActive.Value ? "Active Only" : "Inactive Only");
            if (IsPaid.HasValue)
                filters.Add(IsPaid.Value ? "Paid" : "Unpaid");
            ActiveFiltersText = string.Join(" | ", filters);
            HasActiveFilters = filters.Count != 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating active filters: {ex.Message}");
            ActiveFiltersText = "Error updating filters";
            HasActiveFilters = false;
        }
    }

    public void ClearFilters()
    {
        try
        {
            SelectedGuest = AvailableGuests?.FirstOrDefault();
            SelectedRoom = AvailableRooms?.FirstOrDefault();
            ActiveSelectedIndex = 0;
            PaidSelectedIndex = 0;
            CheckInBookingFrom = null;
            CheckInBookingTo = null;
            CheckOutBookingFrom = null;
            CheckOutBookingTo = null;
            MinAmount = null;
            MaxAmount = null;
            if (ReservationStatuses != null)
            {
                foreach (var status in ReservationStatuses)
                {
                    if (status != null)
                        status.IsSelected = false;
                }
            }
            if (PaymentMethods != null)
            {
                foreach (var method in PaymentMethods)
                {
                    if (method != null)
                        method.IsSelected = false;
                }
            }
            filtersActive = false;
            ReservationsView?.Refresh();
            UpdateFilteredReservationsCount();
            UpdateActiveFilters();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error clearing filters: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
    #endregion
}
#region Commands Implementation - Updated Search Command

public class AddReservationCommand(ReservationListViewModel viewModel, IServiceProvider serviceProvider) : AsyncCommandBase
{
    readonly ReservationListViewModel viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    public override async Task ExecuteAsync(object? parameter)
    {
        try
        {
            var addReservationViewModel = new AddEditReservationViewModel(
                serviceProvider.GetRequiredService<IReservationService>(),
                serviceProvider.GetRequiredService<IGuestService>(),
                serviceProvider.GetRequiredService<IRoomService>());
            var dialog = new AddEditReservationWindow { DataContext = addReservationViewModel };
            if (dialog.ShowDialog() == true)
                await viewModel.LoadReservationsAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error adding reservation: {ex.Message}", "Error",
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}
public class EditReservationCommand(ReservationListViewModel viewModel, IServiceProvider serviceProvider) : AsyncCommandBase
{
    readonly ReservationListViewModel viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) &&
        viewModel.SelectedReservation != null &&
        viewModel.SelectedReservation.Guest != null &&
        viewModel.SelectedReservation.Room != null;
    public override async Task ExecuteAsync(object? parameter)
    {
        var selectedReservation = viewModel.SelectedReservation;
        if (selectedReservation?.Guest == null || selectedReservation.Room == null)
        {
            System.Windows.MessageBox.Show("Please select a valid reservation with complete data.", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }
        try
        {
            var reservationDetail = new ReservationDetail
            {
                Id = selectedReservation.Id,
                Guest = selectedReservation.Guest,
                Room = selectedReservation.Room,
                IsActive = selectedReservation.IsActive,
                CheckInBookingDate = selectedReservation.CheckInBookingDate,
                CheckOutBookingDate = selectedReservation.CheckOutBookingDate,
                Status = selectedReservation.Status,
                CheckedInAt = selectedReservation.CheckedInAt,
                CheckedOutAt = selectedReservation.CheckedOutAt,
                CanceledAt = selectedReservation.CanceledAt,
                DiscountRate = selectedReservation.DiscountRate,
                BaseAmountUsd = selectedReservation.BaseAmountUsd,
                FinalAmountUsd = selectedReservation.FinalAmountUsd,
                IsPaid = selectedReservation.IsPaid,
                PaymentMethod = selectedReservation.PaymentMethod
            };
            var editReservationViewModel = new AddEditReservationViewModel(
                serviceProvider.GetRequiredService<IReservationService>(),
                serviceProvider.GetRequiredService<IGuestService>(),
                serviceProvider.GetRequiredService<IRoomService>(),
                reservationDetail);

            var dialog = new AddEditReservationWindow { DataContext = editReservationViewModel };
            if (dialog.ShowDialog() == true)
                await viewModel.LoadReservationsAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error editing reservation: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}

public class DeleteReservationCommand(ReservationListViewModel viewModel, IReservationService reservationService) : AsyncCommandBase
{
    readonly ReservationListViewModel viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    readonly IReservationService reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) &&
        viewModel.SelectedReservation != null;
    public override async Task ExecuteAsync(object? parameter)
    {
        var selectedReservation = viewModel.SelectedReservation;
        if (selectedReservation == null) return;
        var guestName = selectedReservation.GuestDisplayName ?? "Unknown Guest";
        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete the reservation for {guestName}?",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                await reservationService.DeleteReservationAsync(selectedReservation.Id);
                viewModel.Reservations.Remove(selectedReservation);
                viewModel.ReservationsView?.Refresh();
                viewModel.UpdateFilteredReservationsCount();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error deleting reservation: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}

public class CheckInCommand(ReservationListViewModel viewModel, IReservationService reservationService) : AsyncCommandBase
{
    readonly ReservationListViewModel viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    readonly IReservationService reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) &&
        viewModel.SelectedReservation != null &&
        viewModel.SelectedReservation.Status == ReservationStatus.Created &&
        viewModel.SelectedReservation.Guest != null &&
        viewModel.SelectedReservation.Room != null;
    public override async Task ExecuteAsync(object? parameter)
    {
        var selectedReservation = viewModel.SelectedReservation;
        if (selectedReservation?.Guest == null || selectedReservation.Room == null)
        {
            System.Windows.MessageBox.Show("Cannot check in: Missing guest or room information.", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return;
        }
        var guestName = selectedReservation.GuestDisplayName ?? "Unknown Guest";
        var result = System.Windows.MessageBox.Show(
            $"Check in {guestName}?",
            "Check In Guest",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                viewModel.IsLoading = true;

                // Fetch the full reservation to ensure all references are populated
                var existingReservation = await reservationService.GetReservationAsync(selectedReservation.Id);
                if (existingReservation?.Guest == null || existingReservation.Room == null)
                {
                    System.Windows.MessageBox.Show("Unable to load complete reservation data.", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
                var reservationDetail = new ReservationDetail
                {
                    Id = existingReservation.Id,
                    Guest = existingReservation.Guest,
                    Room = existingReservation.Room,
                    IsActive = existingReservation.IsActive,
                    CheckInBookingDate = existingReservation.CheckInBookingDate,
                    CheckOutBookingDate = existingReservation.CheckOutBookingDate,
                    Status = ReservationStatus.CheckedIn,
                    CheckedInAt = DateTime.UtcNow,
                    CheckedOutAt = null,
                    CanceledAt = null,
                    DiscountRate = existingReservation.DiscountRate,
                    BaseAmountUsd = existingReservation.BaseAmountUsd,
                    FinalAmountUsd = existingReservation.FinalAmountUsd,
                    IsPaid = existingReservation.IsPaid,
                    PaymentMethod = existingReservation.PaymentMethod
                };
                await reservationService.UpdateReservationAsync(reservationDetail);
                await viewModel.LoadReservationsAsync();
                System.Windows.MessageBox.Show(
                    $"{guestName} has been checked in.",
                    "Check-in Complete",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error checking in guest: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                viewModel.IsLoading = false;
            }
        }
    }
}

public class CheckOutCommand(ReservationListViewModel viewModel, IReservationService reservationService) : AsyncCommandBase
{
    readonly ReservationListViewModel viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    readonly IReservationService reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) &&
        viewModel.SelectedReservation != null &&
        viewModel.SelectedReservation.Status == ReservationStatus.CheckedIn &&
        viewModel.SelectedReservation.Guest != null &&
        viewModel.SelectedReservation.Room != null;
    public override async Task ExecuteAsync(object? parameter)
    {
        var selectedReservation = viewModel.SelectedReservation;
        if (selectedReservation?.Guest == null || selectedReservation.Room == null)
        {
            System.Windows.MessageBox.Show("Cannot check out: Missing guest or room information.", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return;
        }
        var guestName = selectedReservation.GuestDisplayName ?? "Unknown Guest";
        var result = System.Windows.MessageBox.Show(
            $"Check out {guestName}?",
            "Check Out Guest",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                viewModel.IsLoading = true;
                var existingReservation = await reservationService.GetReservationAsync(selectedReservation.Id);
                if (existingReservation?.Guest == null || existingReservation.Room == null)
                {
                    System.Windows.MessageBox.Show("Unable to load complete reservation data.", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
                var reservationDetail = new ReservationDetail
                {
                    Id = existingReservation.Id,
                    Guest = existingReservation.Guest,
                    Room = existingReservation.Room,
                    IsActive = existingReservation.IsActive,
                    CheckInBookingDate = existingReservation.CheckInBookingDate,
                    CheckOutBookingDate = existingReservation.CheckOutBookingDate,
                    Status = ReservationStatus.CheckedOut,
                    CheckedInAt = existingReservation.CheckedInAt,
                    CheckedOutAt = DateTime.UtcNow,
                    CanceledAt = null,
                    DiscountRate = existingReservation.DiscountRate,
                    BaseAmountUsd = existingReservation.BaseAmountUsd,
                    FinalAmountUsd = existingReservation.FinalAmountUsd,
                    IsPaid = existingReservation.IsPaid,
                    PaymentMethod = existingReservation.PaymentMethod
                };
                await reservationService.UpdateReservationAsync(reservationDetail);
                await viewModel.LoadReservationsAsync();
                System.Windows.MessageBox.Show(
                    $"{guestName} has been checked out.",
                    "Check-out Complete",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error checking out guest: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                viewModel.IsLoading = false;
            }
        }
    }
}

public class CancelReservationCommand(ReservationListViewModel viewModel, IReservationService reservationService) : AsyncCommandBase
{
    readonly ReservationListViewModel viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    readonly IReservationService reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) &&
        viewModel.SelectedReservation != null &&
        (viewModel.SelectedReservation.Status == ReservationStatus.Created ||
         viewModel.SelectedReservation.Status == ReservationStatus.CheckedIn) &&
        viewModel.SelectedReservation.Guest != null &&
        viewModel.SelectedReservation.Room != null;
    public override async Task ExecuteAsync(object? parameter)
    {
        var selectedReservation = viewModel.SelectedReservation;
        if (selectedReservation?.Guest == null || selectedReservation.Room == null)
        {
            System.Windows.MessageBox.Show("Cannot cancel: Missing guest or room information.", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return;
        }
        var guestName = selectedReservation.GuestDisplayName ?? "Unknown Guest";
        var result = System.Windows.MessageBox.Show(
            $"Cancel the reservation for {guestName}?",
            "Cancel Reservation",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                viewModel.IsLoading = true;
                var existingReservation = await reservationService.GetReservationAsync(selectedReservation.Id);
                if (existingReservation?.Guest == null || existingReservation.Room == null)
                {
                    System.Windows.MessageBox.Show("Unable to load complete reservation data.", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
                var reservationDetail = new ReservationDetail
                {
                    Id = existingReservation.Id,
                    Guest = existingReservation.Guest,
                    Room = existingReservation.Room,
                    IsActive = existingReservation.IsActive,
                    CheckInBookingDate = existingReservation.CheckInBookingDate,
                    CheckOutBookingDate = existingReservation.CheckOutBookingDate,
                    Status = ReservationStatus.Canceled,
                    CheckedInAt = existingReservation.CheckedInAt,
                    CheckedOutAt = null,
                    CanceledAt = DateTime.UtcNow,
                    DiscountRate = existingReservation.DiscountRate,
                    BaseAmountUsd = existingReservation.BaseAmountUsd,
                    FinalAmountUsd = existingReservation.FinalAmountUsd,
                    IsPaid = existingReservation.IsPaid,
                    PaymentMethod = existingReservation.PaymentMethod
                };
                await reservationService.UpdateReservationAsync(reservationDetail);
                await viewModel.LoadReservationsAsync();
                System.Windows.MessageBox.Show(
                    $"Reservation for {guestName} has been canceled.",
                    "Reservation Canceled",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error canceling reservation: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                viewModel.IsLoading = false;
            }
        }
    }
}
public class SearchReservationCommand(ReservationListViewModel viewModel) : AsyncCommandBase
{
    readonly ReservationListViewModel viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    public override Task ExecuteAsync(object? parameter)
    {
        try
        {
            viewModel.ApplyFilters();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error executing search: {ex.Message}", "Search Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        return Task.CompletedTask;
    }
}

public class LoadReservationsCommand : AsyncCommandBase
{
    readonly ReservationListViewModel viewModel;
    public LoadReservationsCommand(ReservationListViewModel viewModel)
    {
        this.viewModel = viewModel;
    }
    public override async Task ExecuteAsync(object? parameter) => await viewModel.LoadReservationsAsync();
}
public class ClearFiltersReservationCommand(ReservationListViewModel viewModel) : CommandBase
{
    readonly ReservationListViewModel viewModel = viewModel;
    public override void Execute(object? parameter) => viewModel.ClearFilters();
}
public class RefreshReservationCommand(ReservationListViewModel viewModel) : AsyncCommandBase
{
    readonly ReservationListViewModel viewModel = viewModel;
    public override async Task ExecuteAsync(object? parameter) => await viewModel.LoadReservationsAsync();
}
#endregion