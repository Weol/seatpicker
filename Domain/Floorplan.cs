namespace Seatpicker.Domain;

public class Floorplan
{
    public int Id { get; set; }

    public ICollection<Seat> Seats { get; }

    public Backdrop Backdrop { get; set; }

    public Seat FindSeat(int seatId)
    {
        var seat = Seats.FirstOrDefault(seat => seat.Id == seatId);
        if (seat == null) throw new SeatNotFoundException(seatId);
        return seat;
    }
}

public class Seat
{
    public int Id { get; }

    public Seat? PositionRelativeTo { get; set; }

    public User? User { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }
}

public record Backdrop(byte[] Pdf);

public class SeatNotFoundException : DomainException
{
    public SeatNotFoundException(int seatId) : base($"No seat with id {seatId} was found") { }

    public int SeatId { get; }
}