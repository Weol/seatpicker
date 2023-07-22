using Seatpicker.Domain;

namespace Seatpicker.Application.Features.Seats;

public class SeatNotFoundException : DomainException
{
    public required Guid SeatId { get; init; }

    public override string Message => $"Seat with id {SeatId} not found";
}

public class DuplicateSeatReservationException : DomainException
{
    public required Seat AttemptedSeatReservation { get; init; }
    public required Seat ExistingSeatReservation { get; init; }
    public required User User { get; init; }

    public override string Message =>
        $"{User} tried to reserve seat {AttemptedSeatReservation} but already has a reservation on seat {ExistingSeatReservation}";
}