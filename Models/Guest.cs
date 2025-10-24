// Ignore Spelling: Fixup

using System.ComponentModel.DataAnnotations;
namespace NovaStayHotel;
public class Guest
{
    #region Properties
    [Key]
    public long Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool IsMale { get; set; }
    public Nationality Nationality { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? EmailAddress { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    #endregion
}

public record GuestDetail
{
    #region Properties
    public long Id { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required bool IsMale { get; init; }
    public required Nationality Nationality { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string PhoneNumber { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? PassportNumber { get; init; }
    #endregion

    #region Fixup & Validate

    public void Fixup() => this.FixupStringProperties();

    public const int MinAge = 18;
    public const int MaxAge = 120;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
            throw new ArgumentException("First name is required");

        if (string.IsNullOrWhiteSpace(MiddleName))
            throw new ArgumentException("Middle name is required");

        if (string.IsNullOrWhiteSpace(LastName))
            throw new ArgumentException("Last name is required");

        if (Nationality == default)
            throw new ArgumentException("Nationality is required");

        if (string.IsNullOrWhiteSpace(PhoneNumber))
            throw new ArgumentException("Phone number is required");

        if (DateOfBirth == default)
            throw new ArgumentException("Date of Birth is required");

        int age = ComputeAge(DateOfBirth);
        if (age < MinAge || age > MaxAge)
            throw new ArgumentException($"Age must be between {MinAge} and {MaxAge}");
    }

    static int ComputeAge(DateOnly DateOfBirth)
    {
        int age = DateTime.Today.Year - DateOfBirth.Year;
        if (DateOfBirth > DateOnly.FromDateTime(DateTime.Today).AddYears(-age))
            age--;
        return age;
    }
    #endregion
}

public record GuestReference
{
    #region Properties
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required string PhoneNumber { get; init; }
    #endregion

    #region Project
    public static GuestReference ProjectGuestReference(Guest x)
    {
        return new GuestReference
        {
            Id = x.Id,
            Name = x.FirstName + " " + x.MiddleName + " " + x.LastName,
            PhoneNumber = x.PhoneNumber
        };
    }
    #endregion
}

public class GuestCriteria
{
    #region Properties
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    #endregion

    #region Fixup
    public void Fixup() => this.FixupStringProperties();
    #endregion
}
