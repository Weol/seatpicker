using Microsoft.EntityFrameworkCore;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class SeatpickerContext : DbContext
{
    public DbSet<Reservation> Reservations { get; set; }

    public DbSet<Seat> Seat { get; set;  }
    
    public DbSet<Floorplan> Floorplans { get; set;  }
}

public class Reservation
{
}

public class Floorplan 
{
}

public class Seat
{
}
