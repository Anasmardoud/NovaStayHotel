using Microsoft.Extensions.DependencyInjection;
using NovaStayHotel.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace NovaStayHotel;

public class GuestListViewModel : BaseViewModel
{
    #region Construction
    readonly IGuestService guestService;
    readonly IServiceProvider serviceProvider;
    readonly NavigationService<HomeViewModel> homeNavService;
    public GuestListViewModel(
        IGuestService guestService,
        IServiceProvider serviceProvider,
        NavigationService<HomeViewModel> homeNavService)
    {
        this.guestService = guestService;
        this.serviceProvider = serviceProvider;
        this.homeNavService = homeNavService;
        InitializeMultiSelectCollections();
        GuestsView = CollectionViewSource.GetDefaultView(Guests);
        GuestsView.Filter = FilterGuests;
        NavigateToHomeCommand = new NavigateCommand<HomeViewModel>(homeNavService);
        LoadGuestsCommand = new LoadGuestsCommand(this);
        AddGuestCommand = new AddGuestCommand(this, serviceProvider);
        EditGuestCommand = new EditGuestCommand(this, serviceProvider);
        DeleteGuestCommand = new DeleteGuestCommand(this, guestService);
        ClearFiltersCommand = new ClearGuestFiltersCommand(this);
        RefreshCommand = new RefreshGuestsCommand(this);
        SearchCommand = new SearchGuestsCommand(this);
        LoadGuestsCommand.Execute(null);
    }
    #endregion

    #region Properties
    public ObservableCollection<GuestDetailViewModel> Guests { get; set; } = [];
    public ICollectionView GuestsView { get; set; }
    public ObservableCollection<SelectableItem<Nationality>> Nationalities { get; set; } = new ObservableCollection<SelectableItem<Nationality>>();

    string? nameFilter;
    public string? NameFilter
    {
        get => nameFilter;
        set
        {
            nameFilter = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }

    string? phoneFilter;
    public string? PhoneFilter
    {
        get => phoneFilter;
        set
        {
            phoneFilter = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }

    int? minAge;
    public int? MinAge
    {
        get => minAge;
        set
        {
            if (value.HasValue && (value < GuestDetail.MinAge || value > GuestDetail.MaxAge))
                return;
            minAge = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }

    int? maxAge;
    public int? MaxAge
    {
        get => maxAge;
        set
        {
            if (value.HasValue && (value < GuestDetail.MinAge || value > GuestDetail.MaxAge))
                return;
            maxAge = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }

    bool? isMale;
    public bool? IsMale
    {
        get => isMale;
        set
        {
            isMale = value;
            OnPropertyChanged();
            UpdateActiveFilters();
        }
    }

    int genderSelectedIndex;
    public int GenderSelectedIndex
    {
        get => genderSelectedIndex;
        set
        {
            genderSelectedIndex = value;
            OnPropertyChanged();
            IsMale = value switch
            {
                1 => true,
                2 => false,
                _ => null
            };
        }
    }

    GuestDetailViewModel? selectedGuest;
    public GuestDetailViewModel? SelectedGuest
    {
        get => selectedGuest;
        set
        {
            selectedGuest = value;
            OnPropertyChanged();
            EditGuestCommand.OnCanExecuteChanged();
            DeleteGuestCommand.OnCanExecuteChanged();
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

    int filteredGuestsCount;
    public int FilteredGuestsCount
    {
        get => filteredGuestsCount;
        set
        {
            filteredGuestsCount = value;
            OnPropertyChanged();
        }
    }

    public static string[] GenderOptions => ["All", "Male", "Female"];

    public IEnumerable<Nationality> SelectedNationalities => Nationalities.Where(x => x.IsSelected).Select(x => x.Value);
    #endregion

    #region Commands
    public ICommand NavigateToHomeCommand { get; }
    public AsyncCommandBase LoadGuestsCommand { get; }
    public AsyncCommandBase AddGuestCommand { get; }
    public AsyncCommandBase EditGuestCommand { get; }
    public AsyncCommandBase DeleteGuestCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public AsyncCommandBase RefreshCommand { get; }
    public AsyncCommandBase SearchCommand { get; }
    #endregion

    #region Methods
    void InitializeMultiSelectCollections()
    {
        foreach (var nationality in Enum.GetValues<Nationality>())
        {
            var item = new SelectableItem<Nationality>(nationality, nationality.ToString());
            item.PropertyChanged += (s, e) => UpdateActiveFilters();
            Nationalities.Add(item);
        }
    }

    public async Task LoadGuestsAsync()
    {
        try
        {
            IsLoading = true;
            var criteria = new GuestCriteria
            {
                Name = NameFilter,
                PhoneNumber = PhoneFilter
            };
            criteria.Fixup();
            var guests = await guestService.GetGuestsAsync(criteria);
            var filteredGuests = guests.AsEnumerable();
            if (MinAge.HasValue || MaxAge.HasValue)
            {
                filteredGuests = filteredGuests.Where(g =>
                {
                    int age = ComputeAge(g.DateOfBirth);
                    return (!MinAge.HasValue || age >= MinAge.Value) &&
                           (!MaxAge.HasValue || age <= MaxAge.Value);
                });
            }
            if (IsMale.HasValue)
                filteredGuests = filteredGuests.Where(g => g.IsMale == IsMale.Value);
            var selectedNats = SelectedNationalities.ToList();
            if (selectedNats.Any())
                filteredGuests = filteredGuests.Where(g => selectedNats.Contains(g.Nationality));
            Guests.Clear();
            foreach (var guest in filteredGuests)
                Guests.Add(new GuestDetailViewModel(guest, guestService));
            FilteredGuestsCount = Guests.Count;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error loading guests: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    bool FilterGuests(object item) => true;

    void UpdateActiveFilters()
    {
        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(NameFilter))
            filters.Add($"Name: {NameFilter}");
        if (!string.IsNullOrWhiteSpace(PhoneFilter))
            filters.Add($"Phone: {PhoneFilter}");
        if (MinAge.HasValue || MaxAge.HasValue)
        {
            var min = MinAge?.ToString() ?? GuestDetail.MinAge.ToString();
            var max = MaxAge?.ToString() ?? GuestDetail.MaxAge.ToString();
            filters.Add($"Age: {min}-{max}");
        }
        var selectedNationalityNames = SelectedNationalities.Select(n => n.ToString()).ToList();
        if (selectedNationalityNames.Count != 0)
            filters.Add($"Nationalities: {string.Join(", ", selectedNationalityNames)}");
        if (IsMale.HasValue)
            filters.Add(IsMale.Value ? "Male" : "Female");
        ActiveFiltersText = string.Join(" | ", filters);
        HasActiveFilters = filters.Count != 0;
    }

    public void ClearFilters()
    {
        NameFilter = null;
        PhoneFilter = null;
        MinAge = null;
        MaxAge = null;
        foreach (var nationality in Nationalities)
            nationality.IsSelected = false;
        GenderSelectedIndex = 0;
        UpdateActiveFilters();
    }
    public bool IsValidAge(int? age) => !age.HasValue || (age >= GuestDetail.MinAge && age <= GuestDetail.MaxAge);

    public string GetAgeValidationMessage() => $"Age must be between {GuestDetail.MinAge} and {GuestDetail.MaxAge}";
    static int ComputeAge(DateOnly dateOfBirth)
    {
        int age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.Today).AddYears(-age))
            age--;
        return age;
    }
    #endregion
}

#region Commands Implementation
public class LoadGuestsCommand(GuestListViewModel viewModel) : AsyncCommandBase
{
    readonly GuestListViewModel viewModel = viewModel;
    public override async Task ExecuteAsync(object? parameter) => await viewModel.LoadGuestsAsync();
}

public class AddGuestCommand(GuestListViewModel viewModel, IServiceProvider serviceProvider) : AsyncCommandBase
{
    readonly GuestListViewModel viewModel = viewModel;
    readonly IServiceProvider serviceProvider = serviceProvider;
    public override async Task ExecuteAsync(object? parameter)
    {
        var viewModelFactory = serviceProvider.GetRequiredService<Func<AddEditGuestViewModel>>();
        var addGuestViewModel = viewModelFactory();

        var dialog = new AddEditGuestWindow { DataContext = addGuestViewModel };
        if (dialog.ShowDialog() == true)
            await viewModel.LoadGuestsAsync();

    }
}

public class EditGuestCommand(GuestListViewModel viewModel, IServiceProvider serviceProvider) : AsyncCommandBase
{
    readonly GuestListViewModel viewModel = viewModel;
    readonly IServiceProvider serviceProvider = serviceProvider;
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) && viewModel.SelectedGuest != null;
    public override async Task ExecuteAsync(object? parameter)
    {
        if (viewModel.SelectedGuest == null) return;
        try
        {
            var guestDetail = new GuestDetail
            {
                Id = viewModel.SelectedGuest.Id,
                FirstName = viewModel.SelectedGuest.FirstName,
                MiddleName = viewModel.SelectedGuest.MiddleName,
                LastName = viewModel.SelectedGuest.LastName,
                IsMale = viewModel.SelectedGuest.IsMale,
                Nationality = viewModel.SelectedGuest.Nationality,
                DateOfBirth = viewModel.SelectedGuest.DateOfBirth,
                PhoneNumber = viewModel.SelectedGuest.PhoneNumber,
                EmailAddress = viewModel.SelectedGuest.EmailAddress,
                PassportNumber = viewModel.SelectedGuest.PassportNumber
            };
            var editGuestViewModel = new AddEditGuestViewModel(serviceProvider.GetRequiredService<IGuestService>(), guestDetail);
            var dialog = new AddEditGuestWindow { DataContext = editGuestViewModel };
            if (dialog.ShowDialog() == true)
                await viewModel.LoadGuestsAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error editing guest: {ex.Message}", "Error",
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}

public class DeleteGuestCommand(GuestListViewModel viewModel, IGuestService guestService) : AsyncCommandBase
{
    readonly GuestListViewModel viewModel = viewModel;
    readonly IGuestService guestService = guestService;

    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) && viewModel.SelectedGuest != null;

    public override async Task ExecuteAsync(object? parameter)
    {
        if (viewModel.SelectedGuest == null) return;
        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete guest {viewModel.SelectedGuest.FullName}?",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                await guestService.DeleteGuestAsync(viewModel.SelectedGuest.Id);
                viewModel.Guests.Remove(viewModel.SelectedGuest);
                viewModel.FilteredGuestsCount = viewModel.Guests.Count;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error deleting guest: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}

public class ClearGuestFiltersCommand(GuestListViewModel viewModel) : CommandBase
{
    readonly GuestListViewModel viewModel = viewModel;
    public override void Execute(object? parameter) => viewModel.ClearFilters();
}

public class RefreshGuestsCommand(GuestListViewModel viewModel) : AsyncCommandBase
{
    public override async Task ExecuteAsync(object? parameter) => await viewModel.LoadGuestsAsync();
}

public class SearchGuestsCommand(GuestListViewModel viewModel) : AsyncCommandBase
{
    public override async Task ExecuteAsync(object? parameter) => await viewModel.LoadGuestsAsync();
}
#endregion