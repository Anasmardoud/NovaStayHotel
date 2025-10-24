namespace NovaStayHotel;

public class AddEditGuestViewModel : BaseAddEditViewModel<GuestDetail, IGuestService>
{
    #region Construction
    protected override string EntityName => "Guest";
    public AddEditGuestViewModel(IGuestService guestService) : base(guestService) { }

    public AddEditGuestViewModel(IGuestService guestService, GuestDetail guestToEdit)
        : base(guestService, guestToEdit) { }
    #endregion

    #region Properties
    string firstName = "John";
    public string FirstName
    {
        get => firstName;
        set
        {
            if (firstName == value) return;
            firstName = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(FirstName));
            UpdateCanSave();
        }
    }

    string middleName = "Michael";
    public string MiddleName
    {
        get => middleName;
        set
        {
            if (middleName == value) return;
            middleName = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(MiddleName));
            UpdateCanSave();
        }
    }

    string lastName = "Doe";
    public string LastName
    {
        get => lastName;
        set
        {
            if (lastName == value) return;
            lastName = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(LastName));
            UpdateCanSave();
        }
    }

    bool isMale = true;
    public bool IsMale
    {
        get => isMale;
        set
        {
            if (isMale == value) return;
            isMale = value;
            OnPropertyChanged();
        }
    }
    Nationality nationality = Nationality.Lebanon;
    public Nationality Nationality
    {
        get => nationality;
        set
        {
            if (nationality == value) return;
            nationality = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(Nationality));
            UpdateCanSave();
        }
    }
    DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-25));
    public DateOnly DateOfBirth
    {
        get => dateOfBirth;
        set
        {
            if (dateOfBirth == value) return;
            dateOfBirth = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DateOfBirthDateTime));
            ValidateProperty(value, nameof(DateOfBirth));
            UpdateCanSave();
        }
    }
    public DateTime DateOfBirthDateTime
    {
        get => DateOfBirth.ToDateTime(TimeOnly.MinValue);
        set => DateOfBirth = DateOnly.FromDateTime(value);
    }

    string phoneNumber = "+961 70 123 456";
    public string PhoneNumber
    {
        get => phoneNumber;
        set
        {
            if (phoneNumber == value) return;
            phoneNumber = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(PhoneNumber));
            UpdateCanSave();
        }
    }

    string? emailAddress = "john.doe@example.com";
    public string? EmailAddress
    {
        get => emailAddress;
        set
        {
            if (emailAddress == value) return;
            emailAddress = value;
            OnPropertyChanged();
            ValidateProperty(value, nameof(EmailAddress));
            UpdateCanSave();
        }
    }

    string? passportNumber;
    public string? PassportNumber
    {
        get => passportNumber;
        set
        {
            if (passportNumber == value) return;
            passportNumber = value;
            OnPropertyChanged();
        }
    }

    public static Array Nationalities => Enum.GetValues<Nationality>();
    public static Array GenderOptions => new[] { "Male", "Female" };
    #endregion

    #region Abstract Method Implementations
    protected override void SetDefaultsWithoutValidation()
    {
        firstName = "John";
        middleName = "Michael";
        lastName = "Doe";
        isMale = true;
        nationality = Nationality.Lebanon;
        dateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-25));
        phoneNumber = "+961 70 123 456";
        emailAddress = "john.doe@example.com";
        passportNumber = null;

        OnPropertyChanged(nameof(FirstName));
        OnPropertyChanged(nameof(MiddleName));
        OnPropertyChanged(nameof(LastName));
        OnPropertyChanged(nameof(IsMale));
        OnPropertyChanged(nameof(Nationality));
        OnPropertyChanged(nameof(DateOfBirth));
        OnPropertyChanged(nameof(DateOfBirthDateTime));
        OnPropertyChanged(nameof(PhoneNumber));
        OnPropertyChanged(nameof(EmailAddress));
        OnPropertyChanged(nameof(PassportNumber));
    }

    protected override void LoadExistingDataWithoutValidation(GuestDetail guestToEdit)
    {
        firstName = guestToEdit.FirstName;
        middleName = guestToEdit.MiddleName;
        lastName = guestToEdit.LastName;
        isMale = guestToEdit.IsMale;
        nationality = guestToEdit.Nationality;
        dateOfBirth = guestToEdit.DateOfBirth;
        phoneNumber = guestToEdit.PhoneNumber;
        emailAddress = guestToEdit.EmailAddress;
        passportNumber = guestToEdit.PassportNumber;

        OnPropertyChanged(nameof(FirstName));
        OnPropertyChanged(nameof(MiddleName));
        OnPropertyChanged(nameof(LastName));
        OnPropertyChanged(nameof(IsMale));
        OnPropertyChanged(nameof(Nationality));
        OnPropertyChanged(nameof(DateOfBirth));
        OnPropertyChanged(nameof(DateOfBirthDateTime));
        OnPropertyChanged(nameof(PhoneNumber));
        OnPropertyChanged(nameof(EmailAddress));
        OnPropertyChanged(nameof(PassportNumber));
    }
    protected override void ValidateProperty(object? value, string propertyName)
    {
        ValidatePropertyBase(value, propertyName, errors =>
        {
            switch (propertyName)
            {
                case nameof(FirstName):
                    if (string.IsNullOrWhiteSpace(FirstName))
                        errors.Add("First name is required");
                    break;

                case nameof(MiddleName):
                    if (string.IsNullOrWhiteSpace(MiddleName))
                        errors.Add("Middle name is required");
                    break;

                case nameof(LastName):
                    if (string.IsNullOrWhiteSpace(LastName))
                        errors.Add("Last name is required");
                    break;

                case nameof(Nationality):
                    if (Nationality == default)
                        errors.Add("Nationality is required");
                    break;

                case nameof(DateOfBirth):
                    if (DateOfBirth == default)
                        errors.Add("Date of Birth is required");
                    else
                    {
                        int age = ComputeAge(DateOfBirth);
                        if (age < GuestDetail.MinAge || age > GuestDetail.MaxAge)
                            errors.Add($"Age must be between {GuestDetail.MinAge} and {GuestDetail.MaxAge}");
                    }
                    break;

                case nameof(PhoneNumber):
                    if (string.IsNullOrWhiteSpace(PhoneNumber))
                        errors.Add("Phone number is required");
                    break;

                case nameof(EmailAddress):
                    if (!string.IsNullOrWhiteSpace(EmailAddress) && !IsValidEmail(EmailAddress))
                        errors.Add("Email address is not in a valid format");
                    break;
            }
        });
    }

    protected override void ValidateAll()
    {
        propertyErrors.Clear();

        ValidateProperty(FirstName, nameof(FirstName));
        ValidateProperty(MiddleName, nameof(MiddleName));
        ValidateProperty(LastName, nameof(LastName));
        ValidateProperty(Nationality, nameof(Nationality));
        ValidateProperty(DateOfBirth, nameof(DateOfBirth));
        ValidateProperty(PhoneNumber, nameof(PhoneNumber));
        ValidateProperty(EmailAddress, nameof(EmailAddress));
    }

    protected override Task<GuestDetail> CreateEntityAsync()
    {
        return Task.FromResult(new GuestDetail
        {
            Id = originalEntityId,
            FirstName = FirstName,
            MiddleName = MiddleName,
            LastName = LastName,
            IsMale = IsMale,
            Nationality = Nationality,
            DateOfBirth = DateOfBirth,
            PhoneNumber = PhoneNumber,
            EmailAddress = EmailAddress,
            PassportNumber = PassportNumber
        });
    }

    protected override async Task UpdateEntityAsync(GuestDetail entity)
    {
        entity.Validate();
        await service.UpdateGuestAsync(entity);
    }

    protected override async Task AddEntityAsync(GuestDetail entity)
    {
        entity.Validate();
        await service.AddGuestAsync(entity);
    }

    protected override long GetEntityId(GuestDetail entity) => entity.Id;
    #endregion

    #region Helper Methods
    static int ComputeAge(DateOnly dateOfBirth)
    {
        int age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.Today).AddYears(-age))
            age--;
        return age;
    }

    static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    #endregion
}