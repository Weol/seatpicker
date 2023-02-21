namespace Seatpicker.Domain;

public class Reservation
{
    public Reservation(Guid id, User? user, DateTime ReservedAt)
    {
        Id = id;
        User = user;
    }

    public Guid Id { get; }

    public User? User { get; set; }

    public DateTime ReservedAt { get; }
}
