namespace Seatpicker.Domain;

public class Seat
{
    public Guid Id { get; init; }

    public User? User { get; set; }

    public string Title { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public DateTime? ReservedAt { get; set; }
}