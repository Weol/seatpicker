using Seatpicker.Application;
using Shared;

namespace Seatpicker.Domain;

#pragma warning disable CS1998 // Disable warning about async methods missing awaits
public class Seat : AggregateRoot
{
    public string Title { get; set; }

    public Bounds Bounds { get; set; }

    public User? ReservedBy { get; private set; }

    public void Reserve(User reserveBy)
    {
        if (reserveBy == ReservedBy) return;

        if (ReservedBy is not null) throw new SeatAlreadyReservedException { SeatId = Id };

        var evt = new SeatReserved(Id, reserveBy);
        Raise(evt);
        Apply(evt);
    }

    public void Unreserve(User unreservedBy)
    {
        if (ReservedBy is null) return;

        var evt = new SeatUnreserved(Id, ReservedBy);
        Raise(evt);
        Apply(evt);
    }

    /**
     * Apply methods automatically used by Marten
     */
    private async Task Apply(SeatCreated evt)
    {
        Id = evt.Id;
        Title = evt.Title;
        Bounds = evt.Bounds;
    }

    private async Task Apply(SeatReserved evt)
    {
        ReservedBy = evt.User;
    }

    private async Task Apply(SeatUnreserved evt)
    {
        ReservedBy = null;
    }
}

public record Bounds(double X, double Y, double Width, double Height);

/**
 * Events
 */
public record SeatCreated(Guid Id, string Title, Bounds Bounds);

public record SeatReserved(Guid Id, User User);

public record SeatUnreserved(Guid Id, User User);

public class SeatAlreadyReservedException : DomainException
{
    public required Guid SeatId { get; init; }

    public override string Message => $"Seat with id {SeatId} is already reserved";
}