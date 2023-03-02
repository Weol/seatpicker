namespace Seatpicker.Domain;

public class Seat
{
    public Guid Id { get; init; }

    public User? User { get; set; }

    public DateTime ReservedAt { get; init;  }
}
