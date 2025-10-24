// Ignore Spelling: Usd Fixup
using System.ComponentModel.DataAnnotations;
namespace NovaStayHotel;
public class Room
{
    #region Properties
    [Key]
    public long Id { get; set; }
    public int Number { get; set; }
    public int FloorNumber { get; set; }
    public RoomType Type { get; set; }
    public RoomStatus Status { get; set; }
    public bool HasBalcony { get; set; }
    public decimal PricePerNightUsd { get; set; }
    public DateTime? LastMaintainedAt { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    #endregion
}

public record RoomDetail
{
    #region Properties
    public long Id { get; init; }
    public required int Number { get; init; }
    public required int FloorNumber { get; init; }
    public required RoomType Type { get; init; }
    public required RoomStatus Status { get; init; }
    public required bool HasBalcony { get; init; }
    public required decimal PricePerNightUsd { get; init; }
    public required DateTime? LastMaintainedAt { get; init; }
    public required string? Description { get; init; }
    #endregion

    #region Fixup & Validate

    public void Fixup() => this.FixupStringProperties();

    public const int MinNumber = 1;
    public const int MaxNumber = 200;
    public const int MinFloor = 0;
    public const int MaxFloor = 20;
    public const decimal MinUsdPrice = 0m;
    public const decimal MaxUsdPrice = 100_000m;

    public void Validate()
    {
        if (Number < MinNumber || Number > MaxNumber)
            throw new ArgumentOutOfRangeException($"Room number must be between {MinNumber} and {MaxNumber}");

        if (FloorNumber < MinFloor || FloorNumber > MaxFloor)
            throw new ArgumentOutOfRangeException($"Floor number must be between {MinFloor} and {MaxFloor}");

        if (Type == default)
            throw new ArgumentException("Room type is required");

        if (Status == default)
            throw new ArgumentException("Room status is required");

        if (PricePerNightUsd < MinUsdPrice || PricePerNightUsd > MaxUsdPrice)
            throw new ArgumentOutOfRangeException($"Room price must be between {MinUsdPrice:#.##} and {MaxUsdPrice:#.##} USD");

        if (LastMaintainedAt != null && LastMaintainedAt > DateTime.UtcNow)
            throw new ArgumentException("Maintenance date cannot be in the future");
    }
    #endregion
}

public record RoomReference
{
    #region Properties
    public required long Id { get; init; }
    public required int Number { get; init; }
    public required int FloorNumber { get; init; }
    public required RoomType RoomType { get; init; }
    #endregion

    #region Project
    public static RoomReference ProjectRoomReference(Room x)
    {
        return new RoomReference
        {
            Id = x.Id,
            Number = x.Number,
            FloorNumber = x.FloorNumber,
            RoomType = x.Type
        };
    }
    #endregion
}

public record RoomCriteria
{
    #region Properties
    public int? MinRoomNumber { get; init; }
    public int? MaxRoomNumber { get; init; }
    public int? MinFloorNumber { get; init; }
    public int? MaxFloorNumber { get; init; }
    public HashSet<RoomType> Types { get; init; } = [];
    public HashSet<RoomStatus> Statuses { get; init; } = [];
    public bool? HasBalcony { get; init; }
    public decimal? MinPricePerNightUsd { get; init; }
    public decimal? MaxPricePerNightUsd { get; init; }
    #endregion

    #region Fixup
    public void Fixup() => this.FixupStringProperties();
    #endregion
}
