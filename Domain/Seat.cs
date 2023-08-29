using System.Diagnostics.CodeAnalysis;
using Shared;

namespace Seatpicker.Domain;

// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CS1998 // Disable warning about async methods missing awaits
#pragma warning disable CS8618 // Disable warning about uninitialized properties
public class Seat : AggregateBase
{
    public string Title { get; set; }

    public Bounds Bounds { get; set; }

    public User? ReservedBy { get; private set; }

    public Seat(Guid id, string title, Bounds bounds, User initiator)
    {
        var evt = new SeatCreated(id, title, bounds, initiator.Id);
        Raise(evt);
        Apply(evt);
    }

    private Seat()
    {
        // Marten needs this
    }

    public void Reserve(User user, ICollection<Seat> seatsReservedByUser, User initiator)
    {
        if (user == ReservedBy) return;

        if (seatsReservedByUser.Any()) throw new DuplicateSeatReservationException(this, seatsReservedByUser, user);

        if (ReservedBy is not null)
            throw new SeatReservationConflictException(this, ReservedBy, user);

        var evt = new SeatReserved(user, initiator.Id);
        Raise(evt);
        Apply(evt);
    }

    public void UnReserve(User initator)
    {
        if (ReservedBy is null) return;

        if (ReservedBy.Id != initator.Id)
            throw new SeatReservationConflictException(this, ReservedBy, initator);

        var evt = new SeatUnreserved(ReservedBy, initator.Id);
        Raise(evt);
        Apply(evt);
    }

    public void MoveReservation(Seat fromSeat, User user, User initiator)
    {
        if (ReservedBy is not null)
            throw new SeatReservationConflictException(this, ReservedBy, user);

        if (fromSeat.ReservedBy is null)
            throw new SeatReservationNotFoundException { Seat = fromSeat };

        if (fromSeat.ReservedBy.Id != user.Id)
            throw new SeatReservationConflictException(this, fromSeat.ReservedBy, user);

        var evt = new SeatReservationMoved(user, fromSeat.Id, Id, initiator.Id);

        fromSeat.Raise(evt);
        fromSeat.Apply(evt);

        Raise(evt);
        Apply(evt);
    }

    public override string ToString() => $"Seat \"{Title}\" ({Id})";

    /**
     * Apply methods automatically used by Marten
     */
    private async void Apply(SeatCreated evt)
    {
        Id = evt.Id;
        Title = evt.Title;
        Bounds = evt.Bounds;
    }

    private async void Apply(SeatReserved evt)
    {
        ReservedBy = evt.User;
    }

    private async void Apply(SeatUnreserved evt)
    {
        ReservedBy = null;
    }

    private async void Apply(SeatReservationMoved evt)
    {
        if (evt.ToSeatId == Id) ReservedBy = evt.User;
        if (evt.FromSeatId == Id) ReservedBy = null;
    }
}

public record Bounds(double X, double Y, double Width, double Height);

/**
 * Events
 */
public record SeatCreated(Guid Id, string Title, Bounds Bounds, UserId InitiatorId);

public record SeatReserved(User User, UserId InitiatorId);

public record SeatUnreserved(User User, UserId InitiatorId);

public record SeatReservationMoved(User User, Guid FromSeatId, Guid ToSeatId, UserId InitiatorId);

/**
 * Exceptions
 */
public class SeatReservationConflictException : DomainException
{
    public required Seat Seat { get; init; }
    public required User ReservedUser { get; init; }
    public required User AttemptedUser { get; init; }

    [SetsRequiredMembers]
    public SeatReservationConflictException(Seat seat, User reservedUser, User attemptedUser)
    {
        ReservedUser = reservedUser;
        AttemptedUser = attemptedUser;
        Seat = seat;
    }

    public override string Message => $"{Seat} is reserved by {ReservedUser}, cannot be changed by {AttemptedUser} ";
}

public class SeatReservationNotFoundException : DomainException
{
    public required Seat Seat { get; init; }

    public override string Message => $"{Seat} has no reservation";
}

public class DuplicateSeatReservationException : DomainException
{
    public required Seat AttemptedSeatReservation { get; init; }
    public required ICollection<Seat> ExistingSeatReservations { get; init; }
    public required User User { get; init; }

    [SetsRequiredMembers]
    internal DuplicateSeatReservationException(
        Seat attemptedSeatReservation,
        ICollection<Seat> existingSeatReservations,
        User user)
    {
        AttemptedSeatReservation = attemptedSeatReservation;
        ExistingSeatReservations = existingSeatReservations;
        User = user;
    }

    public override string Message =>
        $"{User} tried to reserve {AttemptedSeatReservation} but already has a reservation on {ExistingSeatReservations}";
}
