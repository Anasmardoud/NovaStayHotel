using System.ComponentModel;
using System.Windows.Input;

namespace NovaStayHotel;

public class GuestDetailViewModel : BaseViewModel, IEditableObject
{
    #region Construction
    readonly IGuestService guestService;
    GuestDetail currentGuest;
    GuestDetail? backupGuest;

    public GuestDetailViewModel(GuestDetail guest, IGuestService guestService)
    {
        this.guestService = guestService;
        currentGuest = guest;
        UpdatePropertiesFromGuest(guest);
        HasChanges = false;
        SaveCommand = new SaveGuestCommand(this);
        CancelCommand = new CancelGuestEditCommand(this);
    }
    #endregion

    #region Properties
    public long Id => currentGuest.Id;

    string firstName = string.Empty;
    public string FirstName
    {
        get => firstName;
        set
        {
            firstName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FullName));
            HasChanges = true;
        }
    }

    string middleName = string.Empty;
    public string MiddleName
    {
        get => middleName;
        set
        {
            middleName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FullName));
            HasChanges = true;
        }
    }

    string lastName = string.Empty;
    public string LastName
    {
        get => lastName;
        set
        {
            lastName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FullName));
            HasChanges = true;
        }
    }

    public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();
    bool isMale;
    public bool IsMale
    {
        get => isMale;
        set
        {
            isMale = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(GenderDisplay));
            HasChanges = true;
        }
    }

    public string GenderDisplay => IsMale ? "Male" : "Female";

    Nationality nationality;
    public Nationality Nationality
    {
        get => nationality;
        set
        {
            nationality = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    DateOnly dateOfBirth;
    public DateOnly DateOfBirth
    {
        get => dateOfBirth;
        set
        {
            dateOfBirth = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Age));
            OnPropertyChanged(nameof(DateOfBirthFormatted));
            HasChanges = true;
        }
    }

    public string DateOfBirthFormatted => DateOfBirth.ToString("yyyy-MM-dd");

    public int Age
    {
        get
        {
            int age = DateTime.Today.Year - DateOfBirth.Year;
            if (DateOfBirth > DateOnly.FromDateTime(DateTime.Today).AddYears(-age))
                age--;
            return age;
        }
    }

    string phoneNumber = string.Empty;
    public string PhoneNumber
    {
        get => phoneNumber;
        set
        {
            phoneNumber = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    string? emailAddress;
    public string? EmailAddress
    {
        get => emailAddress;
        set
        {
            emailAddress = value;
            OnPropertyChanged();
            HasChanges = true;
        }
    }

    string? passportNumber;
    public string? PassportNumber
    {
        get => passportNumber;
        set
        {
            passportNumber = value;
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
    public Array Nationalities => Enum.GetValues<Nationality>();
    public Array GenderOptions => new[] { "Male", "Female" };
    #endregion

    #region Commands
    public AsyncCommandBase SaveCommand { get; }
    public ICommand CancelCommand { get; }
    #endregion

    #region Methods
    void UpdatePropertiesFromGuest(GuestDetail guest)
    {
        firstName = guest.FirstName;
        middleName = guest.MiddleName;
        lastName = guest.LastName;
        isMale = guest.IsMale;
        nationality = guest.Nationality;
        dateOfBirth = guest.DateOfBirth;
        phoneNumber = guest.PhoneNumber;
        emailAddress = guest.EmailAddress;
        passportNumber = guest.PassportNumber;
        OnPropertyChanged(nameof(FirstName));
        OnPropertyChanged(nameof(MiddleName));
        OnPropertyChanged(nameof(LastName));
        OnPropertyChanged(nameof(FullName));
        OnPropertyChanged(nameof(IsMale));
        OnPropertyChanged(nameof(GenderDisplay));
        OnPropertyChanged(nameof(Nationality));
        OnPropertyChanged(nameof(DateOfBirth));
        OnPropertyChanged(nameof(DateOfBirthFormatted));
        OnPropertyChanged(nameof(Age));
        OnPropertyChanged(nameof(PhoneNumber));
        OnPropertyChanged(nameof(EmailAddress));
        OnPropertyChanged(nameof(PassportNumber));
    }

    public void StartEdit()
    {
        if (!IsEditing)
        {
            BeginEdit();
        }
    }

    public async Task SaveAsync()
    {
        if (!IsEditing || !HasChanges) return;
        try
        {
            IsSaving = true;
            var updatedGuest = new GuestDetail
            {
                Id = Id,
                FirstName = FirstName,
                MiddleName = MiddleName,
                LastName = LastName,
                IsMale = IsMale,
                Nationality = Nationality,
                DateOfBirth = DateOfBirth,
                PhoneNumber = PhoneNumber,
                EmailAddress = EmailAddress,
                PassportNumber = PassportNumber
            };
            await guestService.UpdateGuestAsync(updatedGuest);
            currentGuest = updatedGuest;
            EndEdit();
            HasChanges = false;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error saving guest: {ex.Message}", "Error",
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
        {
            ((IEditableObject)this).CancelEdit();
        }
    }
    #endregion

    #region IEditableObject Implementation
    public void BeginEdit()
    {
        if (backupGuest == null)
        {
            backupGuest = new GuestDetail
            {
                Id = Id,
                FirstName = FirstName,
                MiddleName = MiddleName,
                LastName = LastName,
                IsMale = IsMale,
                Nationality = Nationality,
                DateOfBirth = DateOfBirth,
                PhoneNumber = PhoneNumber,
                EmailAddress = EmailAddress,
                PassportNumber = PassportNumber
            };
        }
        IsEditing = true;
    }

    void IEditableObject.CancelEdit()
    {
        if (backupGuest != null)
        {
            UpdatePropertiesFromGuest(backupGuest);
            backupGuest = null;
            HasChanges = false;
        }
        IsEditing = false;
    }

    public void EndEdit()
    {
        backupGuest = null;
        IsEditing = false;
    }
    #endregion
}

#region Commands Implementation
public class SaveGuestCommand : AsyncCommandBase
{
    readonly GuestDetailViewModel viewModel;
    public SaveGuestCommand(GuestDetailViewModel viewModel) => this.viewModel = viewModel;

    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) && viewModel.IsEditing && viewModel.HasChanges;

    public override async Task ExecuteAsync(object? parameter) => await viewModel.SaveAsync();
}

public class CancelGuestEditCommand : CommandBase
{
    readonly GuestDetailViewModel viewModel;
    public CancelGuestEditCommand(GuestDetailViewModel viewModel) => this.viewModel = viewModel;

    public override bool CanExecute(object? parameter) => viewModel.IsEditing;

    public override void Execute(object? parameter) => viewModel.CancelEditCommandAction();
}
#endregion