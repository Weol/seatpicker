using Microsoft.EntityFrameworkCore;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class SeatpickerContext : DbContext
{
    public DbSet<Lan> Lans { get; set; }
}

public class Lan
{
    public int Id { get; set; }

    public IList<Floorplan> Floorplans { get; set; }
}

public class Floorplan
{
    public int Id { get; set; }

    public byte[] Background { get; set; }

    public List<Seat> Seats { get; set; }
}

public class Seat
{
    public int Id { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public string UserId { get; set; }

    public string UserNick { get; set; }

    public string UserAvatar { get; set; }
}