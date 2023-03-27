using Shared;

namespace Seatpicker.Domain;

public class Seat : Entity<Guid>
{
    public User? User { get; set; }

    public string Title { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public DateTime? ReservedAt { get; set; }

    public void Reserve(User user)
    {
        User = user;
        Raise(new SeatReservedEvent(Id, user));
    }


}

public record SeatReservedEvent(Guid SeatId, User User) : IDomainEvent;

public record SeatUnreservedEvent(Guid SeatId, User User) : IDomainEvent;

public record SeatReservationSwitch(Guid previousSeatId, Guid newSeatId, User User) : IDomainEvent;
