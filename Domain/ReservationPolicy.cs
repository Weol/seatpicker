using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Seatpicker.Domain;

public static class ReservationPolicy
{
    public static void EnsureCanReserve(User user, Seat seatToReserve, ImmutableList<Seat> seatsReservedByUser)
    {
        if (seatsReservedByUser.Count > 0)
            throw new DuplicateSeatReservationException(seatToReserve, seatsReservedByUser, user);
    }
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