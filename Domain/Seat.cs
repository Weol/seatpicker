using System.Diagnostics.CodeAnalysis;
using Shared;

namespace Seatpicker.Domain;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedParameter.Local
#pragma warning disable CS1998 // Disable warning about async methods missing awaits
#pragma warning disable CS8618 // Disable warning about uninitialized properties
public class Seat : AggregateBase
{
    public Guid LanId { get; private set; }

    public string Title { get; private set; }

    public Bounds Bounds { get; private set; }

    public UserId? ReservedBy { get; private set; }

    public Seat(Guid id, Lan lan, string title, Bounds bounds, User initiator)
    {
        if (title.Length == 0) throw new ArgumentOutOfRangeException(nameof(title), title, "Title cannot be empty");

        var evt = new SeatCreated(id, lan.Id, title, bounds, initiator.Id);
        Raise(evt);
        Apply(evt);
    }

    // ReSharper disable once UnusedMember.Local
    private Seat()
    {
        // Marten needs this
    }

    public void SetTitle(string title, User initiator)
    {
        if (title.Length == 0) throw new ArgumentOutOfRangeException(nameof(title), title, "Title cannot be empty");

        var evt = new SeatTitleChanged(Id, title, initiator.Id);

        Raise(evt);
        Apply(evt);
    }

    public void SetBounds(Bounds bounds, User initiator)
    {
        var evt = new SeatBoundsChanged(Id, bounds, initiator.Id);

        Raise(evt);
        Apply(evt);
    }

    public void Archive(User initiator)
    {
        var evt = new SeatArchived(Id, initiator.Id);

        Raise(evt);
        Apply(evt);
    }

    public void MakeReservation(User user, int numSeatsReservedByUser)
    {
        if (user.Id == ReservedBy) return;

        if (numSeatsReservedByUser > 0) throw new DuplicateSeatReservationException(this, numSeatsReservedByUser, user);

        if (ReservedBy is not null) throw new SeatReservationConflictException(this, ReservedBy, user.Id);

        var evt = new SeatReservationMade(Id, user.Id);
        Raise(evt);
        Apply(evt);
    }

    public void MakeReservationFor(User user, int seatsReservedByUser, User madeBy)
    {
        if (user.Id == ReservedBy) return;

        if (seatsReservedByUser > 0) throw new DuplicateSeatReservationException(this, seatsReservedByUser, user);

        if (ReservedBy is not null) throw new SeatReservationConflictException(this, ReservedBy, user.Id);

        var evt = new SeatReservationMadeFor(Id, user.Id, madeBy.Id);
        Raise(evt);
        Apply(evt);
    }

    public void RemoveReservation(User initiator)
    {
        if (ReservedBy is null) return;

        if (ReservedBy != initiator.Id) throw new SeatReservationConflictException(this, ReservedBy, initiator.Id);

        var evt = new SeatReservationRemoved(Id, ReservedBy);
        Raise(evt);
        Apply(evt);
    }

    public void RemoveReservationFor(User removedBy)
    {
        if (ReservedBy is null) return;

        var evt = new SeatReservationRemovedFor(Id, ReservedBy, removedBy.Id);
        Raise(evt);
        Apply(evt);
    }

    public void MoveReservation(User user, Seat fromSeat)
    {
        if (ReservedBy is not null) throw new SeatReservationConflictException(this, ReservedBy, user.Id);

        if (fromSeat.ReservedBy is null) throw new SeatReservationNotFoundException { Seat = fromSeat };

        if (fromSeat.ReservedBy != user.Id)
            throw new SeatReservationConflictException(this, fromSeat.ReservedBy, user.Id);

        var evt = new SeatReservationMoved(Id, user.Id, fromSeat.Id, Id);

        fromSeat.Raise(evt);
        fromSeat.Apply(evt);

        Raise(evt);
        Apply(evt);
    }

    public void MoveReservationFor(Seat fromSeat, User movedBy)
    {
        if (ReservedBy is not null) throw new SeatReservationConflictException(this, ReservedBy, movedBy.Id);

        if (fromSeat.ReservedBy is null) throw new SeatReservationNotFoundException { Seat = fromSeat };

        var evt = new SeatReservationMovedFor(Id, fromSeat.ReservedBy, fromSeat.Id, Id, movedBy.Id);

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
        LanId = evt.LanId;
        Title = evt.Title;
        Bounds = evt.Bounds;
    }

    private void Apply(SeatReservationMade evt)
    {
        ReservedBy = evt.UserId;
    }

    private void Apply(SeatReservationMadeFor evt)
    {
        ReservedBy = evt.UserId;
    }

    private void Apply(SeatReservationRemoved evt)
    {
        ReservedBy = null;
    }

    private void Apply(SeatReservationRemovedFor evt)
    {
        ReservedBy = null;
    }

    private void Apply(SeatReservationMoved evt)
    {
        if (evt.ToSeatId == Id) ReservedBy = evt.UserId;
        if (evt.FromSeatId == Id) ReservedBy = null;
    }

    private void Apply(SeatReservationMovedFor evt)
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
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero");

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override string ToString() =>
        $"Bounds {nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Width)}: {Width}, {nameof(Height)}: {Height}";
}

/**
 * Events
 */
public record SeatCreated(Guid Id, Guid LanId, string Title, Bounds Bounds, UserId CreatedBy) : IEvent;

public record SeatTitleChanged(Guid LanId, string Title, UserId ChangedBy) : IEvent;

public record SeatBoundsChanged(Guid LanId, Bounds Bounds, UserId ChangedBy) : IEvent;

public record SeatReservationMade(Guid LanId, UserId UserId) : IEvent;

public record SeatReservationRemoved(Guid LanId, UserId UserId) : IEvent;

public record SeatReservationMoved(Guid LanId, UserId UserId, Guid FromSeatId, Guid ToSeatId) : IEvent;

public record SeatReservationMadeFor(Guid LanId, UserId UserId, UserId MadeBy) : IEvent;

public record SeatReservationRemovedFor(Guid LanId, UserId UserId, UserId RemovedBy) : IEvent;

public record SeatReservationMovedFor(Guid LanId, UserId UserId, Guid FromSeatId, Guid ToSeatId, UserId MovedBy) : IEvent;

public record SeatArchived(Guid LanId, UserId ArchivedBy) : IEvent;

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

    protected override string ErrorMessage =>
        $"{Seat} is reserved by {ReservedUser}, cannot be changed by {AttemptedUser} ";
}

public class SeatReservationNotFoundException : DomainException
{
    public required Seat Seat { get; init; }

    protected override string ErrorMessage => $"{Seat} has no reservation";
}

public class DuplicateSeatReservationException : DomainException
{
    public required Seat AttemptedSeatReservation { get; init; }
    public required int NumExistingSeatReservations { get; init; }
    public required User User { get; init; }

    [SetsRequiredMembers]
    internal DuplicateSeatReservationException(
        Seat attemptedSeatReservation,
        int numExistingSeatReservations,
        User user)
    {
        AttemptedSeatReservation = attemptedSeatReservation;
        NumExistingSeatReservations = numExistingSeatReservations;
        User = user;
    }

    protected override string ErrorMessage =>
        $"Cannot create reservation for {User} on {AttemptedSeatReservation} because they already have reservations on {NumExistingSeatReservations} seats";
}