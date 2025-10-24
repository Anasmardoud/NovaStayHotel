using Microsoft.EntityFrameworkCore;

namespace NovaStayHotel;

public class NovaStayHotelDataContext(DbContextOptions<NovaStayHotelDataContext> options) : DbContext(options)
{

    public DbSet<Reservation> Reservations { get; set; } = null!;
    public DbSet<Guest> Guests { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
