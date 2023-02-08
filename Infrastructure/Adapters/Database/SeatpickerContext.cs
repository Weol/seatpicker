using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class SeatpickerContext : DbContext
{
    public DbSet<ReservationDao> Reservations { get; set; }

    public DbSet<SeatDao> Seat { get; set;  }
    
    public DbSet<FloorplanDao> Floorplans { get; set;  }
}

public class ReservationDao
{
    [Key]
    public int ReservationId { get; set; }

    [ForeignKey(nameof(Seat))]
    public int SeatId { get; set; }
    public SeatDao Seat;
    
    public string Nick { get; set; }
    
    public string DiscordId { get; set; }
    
    public string DiscordAvatar { get; set; }
}

public class FloorplanDao 
{
    [Key]
    public int FloorplanId { get; set; }

    public byte[] Backdrop { get; set; }
}

public class SeatDao
{
    [Key]
    public int SeatId { get; set; }

    [ForeignKey(nameof(Floorplan))]
    public int FloorplanId { get; set; }
    public FloorplanDao Floorplan;
}
