using System.Diagnostics.CodeAnalysis;
using Shared;

namespace Seatpicker.Domain;

// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CS1998 // Disable warning about async methods missing awaits
#pragma warning disable CS8618 // Disable warning about uninitialized properties
public class Seat : AggregateBase
{
    public string Title { get; private set; }

    public Bounds Bounds { get; private set; }

    public UserId? ReservedBy { get; private set; }

    public bool IsArchived { get; private set; }

    public Seat(Guid id, string title, Bounds bounds, User initiator)
    {
        if (title.Length == 0) throw new ArgumentOutOfRangeException(nameof(title), title, "Title cannot be empty");

        var evt = new SeatCreated(id, title, bounds, initiator.Id);
        Raise(evt);
        Apply(evt);
    }

    private Seat()
    {
        // Marten needs this
    }

    public void SetTitle(string title, User initiator)
    {
        if (title.Length == 0) throw new ArgumentOutOfRangeException(nameof(title), title, "Title cannot be empty");

        var evt = new SeatTitleChanged(title, initiator.Id);

        Raise(evt);
        Apply(evt);
    }

    public void SetBounds(Bounds bounds, User initiator)
    {
        var evt = new SeatBoundsChanged(bounds, initiator.Id);

        Raise(evt);
        Apply(evt);
    }

    public void Archive(User initiator)
    {
        var evt = new SeatArchived(initiator.Id);

        Raise(evt);
        Apply(evt);
    }

    public void Reserve(User user, ICollection<Seat> seatsReservedByUser, User initiator)
    {
        if (user.Id == ReservedBy) return;

        if (seatsReservedByUser.Any()) throw new DuplicateSeatReservationException(this, seatsReservedByUser, user);

        if (ReservedBy is not null)
            throw new SeatReservationConflictException(this, ReservedBy, user.Id);

        var evt = new SeatReserved(user.Id, initiator.Id);
        Raise(evt);
        Apply(evt);
    }

    public void UnReserve(User initator)
    {
        if (ReservedBy is null) return;

        if (ReservedBy != initator.Id)
            throw new SeatReservationConflictException(this, ReservedBy, initator.Id);

        var evt = new SeatUnreserved(ReservedBy, initator.Id);
        Raise(evt);
        Apply(evt);
    }

    public void MoveReservation(Seat fromSeat, User user, User initiator)
    {
        if (ReservedBy is not null)
            throw new SeatReservationConflictException(this, ReservedBy, user.Id);

        if (fromSeat.ReservedBy is null)
            throw new SeatReservationNotFoundException { Seat = fromSeat };

        if (fromSeat.ReservedBy != user.Id)
            throw new SeatReservationConflictException(this, fromSeat.ReservedBy, user.Id);

        var evt = new SeatReservationMoved(user.Id, fromSeat.Id, Id, initiator.Id);

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

    private void Apply(SeatReserved evt)
    {
        ReservedBy = evt.UserId;
    }

    private void Apply(SeatUnreserved evt)
    {
        ReservedBy = null;
    }

    private void Apply(SeatReservationMoved evt)
    {
        if (evt.ToSeatId == Id) ReservedBy = evt.UserId;
        if (evt.FromSeatId == Id) ReservedBy = null;
    }

    private void Apply(SeatTitleChanged evt)
    {
        Title = evt.Title;
    }

    private void Apply(SeatBoundsChanged evt)
    {
        Bounds = evt.Bounds;
    }

    private void Apply(SeatArchived evt)
    {
        IsArchived = true;
    }
}

public class Bounds
{
    public double X { get; }
    public double Y { get; }
    public double Width { get; }
    public double Height { get; }

    public Bounds(double x, double y, double width, double height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than zero");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero");

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override string ToString() => $"Bounds {nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Width)}: {Width}, {nameof(Height)}: {Height}";
}

/**
 * Events
 */
public record SeatCreated(Guid Id, string Title, Bounds Bounds, UserId InitiatorId);

public record SeatTitleChanged(string Title, UserId InitiatorId);

public record SeatBoundsChanged(Bounds Bounds, UserId InitiatorId);

public record SeatReserved(UserId UserId, UserId InitiatorId);

public record SeatUnreserved(UserId UserId, UserId InitiatorId);

public record SeatReservationMoved(UserId UserId, Guid FromSeatId, Guid ToSeatId, UserId InitiatorId);

public record SeatArchived(UserId InitiatorId);

/**
 * Exceptions
 */
public class SeatReservationConflictException : DomainException
{
    public required Seat Seat { get; init; }
    public required UserId ReservedUser { get; init; }
    public required UserId AttemptedUser { get; init; }

    [SetsRequiredMembers]
    public SeatReservationConflictException(Seat seat, UserId reservedUser, UserId attemptedUser)
    {
        ReservedUser = reservedUser;
        AttemptedUser = attemptedUser;
        Seat = seat;
    }

    protected override string ErrorMessage => $"{Seat} is reserved by {ReservedUser}, cannot be changed by {AttemptedUser} ";
}

public class SeatReservationNotFoundException : DomainException
{
    public required Seat Seat { get; init; }

    protected override string ErrorMessage => $"{Seat} has no reservation";
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

    protected override string ErrorMessage =>
        $"{User} tried to reserve {AttemptedSeatReservation} but already has a reservation on {ExistingSeatReservations}";
}